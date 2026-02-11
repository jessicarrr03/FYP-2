using System;
using UnityEngine;

[Serializable]
public class StreakData
{
    public string lastCheckInLocalDate;
    public int currentStreak;
    public int bestStreak;
}

public class StreakManager : MonoBehaviour
{
    public static StreakManager I { get; private set; }

    private const string PrefKey = "STREAK_DATA_V2";

    public StreakData Data { get; private set; } = new StreakData();

    public bool lostStreakToday { get; set; } = false;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        Load();
        ResetIfMissedDay();
    }

    public void CheckInToday()
{
    DateTime today = DateTime.Now.Date;
    string todayStr = today.ToString("yyyy-MM-dd");

    // Already checked in today → do nothing
    if (Data.lastCheckInLocalDate == todayStr)
        return;

    if (string.IsNullOrEmpty(Data.lastCheckInLocalDate))
    {
        Data.currentStreak = 1;
    }
    else
    {
        DateTime last = DateTime.Parse(Data.lastCheckInLocalDate);
        int days = (today - last).Days;

        if (days == 1)
            Data.currentStreak += 1;
        else
            Data.currentStreak = 1;
    }

    Data.lastCheckInLocalDate = todayStr;
    Data.bestStreak = Mathf.Max(Data.bestStreak, Data.currentStreak);
    Save();

    // ✅ Log streak check-in analytics row (once per day)
    if (AnalyticsClient.I != null && StudentIdentity.HasId())
    {
        AnalyticsClient.I.Log(new AnalyticsEvent
        {
            student_id = StudentIdentity.GetId(),
            event_type = "streak_checkin",
            topic = "",
            question_id = "",
            correct = 0,
            attempts = Data.currentStreak, // store current streak value here
            time_spent_sec = 0
        });
    }
}

public void OnLessonPassedToday()
{
    // Ensure streak is checked-in for the day (but only once/day)
    CheckInToday();

    // Log analytics event for lesson completion
    if (AnalyticsClient.I != null && StudentIdentity.HasId())
    {
        AnalyticsClient.I.Log(new AnalyticsEvent
        {
            student_id = StudentIdentity.GetId(),
            event_type = "lesson_passed",
            topic = "UNKNOWN", // we will fix this later using your topic/week system
            question_id = "",
            correct = 1,
            attempts = 1,
            time_spent_sec = 0
        });
    }
}


    private void ResetIfMissedDay()
    {
        if (string.IsNullOrEmpty(Data.lastCheckInLocalDate))
            return;

        DateTime last = DateTime.Parse(Data.lastCheckInLocalDate);
        DateTime today = DateTime.Now.Date;
        int gap = (today - last).Days;

        if (gap > 1)
        {
            Data.currentStreak = 0;
            lostStreakToday = true;
            Save();
            Debug.Log($"[Streak] Reset to 0 (missed {gap - 1} day(s))");
        }
    }

    public bool HasCheckedInToday()
    {
        return Data.lastCheckInLocalDate == DateTime.Now.Date.ToString("yyyy-MM-dd");
    }

    public double SecondsUntilMidnight()
    {
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1);
        return (nextMidnight - now).TotalSeconds;
    }

    public void ForceReset()
    {
        Data.currentStreak = 0;
        Save();
    }

    private void Save()
    {
        PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(PrefKey))
        {
            Data = JsonUtility.FromJson<StreakData>(PlayerPrefs.GetString(PrefKey));
        }
        else
        {
            Data = new StreakData
            {
                lastCheckInLocalDate = "",
                currentStreak = 0,
                bestStreak = 0
            };
            Save();
        }
    }
}
