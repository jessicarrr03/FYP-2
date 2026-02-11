using UnityEngine;
using TMPro;

public class StreakBadgeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private float tickTimer;

    void Start()
    {
        UpdateBadge(true);
    }

    void Update()
    {
        if (Time.time - tickTimer > 1f)
        {
            tickTimer = Time.time;
            UpdateBadge(false);
        }
    }

    public void RefreshNow()
    {
        UpdateBadge(true);
    }

    private void UpdateBadge(bool force)
    {
        if (label == null || StreakManager.I == null)
        {
            return;
        }

        int current = StreakManager.I.Data.currentStreak;
        int best = StreakManager.I.Data.bestStreak;
        bool doneToday = StreakManager.I.HasCheckedInToday();

        int secs = Mathf.Max(0, (int)StreakManager.I.SecondsUntilMidnight());
        int h = secs / 3600, m = (secs % 3600) / 60, s = secs % 60;
        string timeStr = $"{h:D2}:{m:D2}:{s:D2}";

        string status = doneToday
            ? $" Done for today! Good job! \n Resets in {timeStr}"
            : $" Do 1 lesson today! Quick! \n {timeStr} left";

        label.text = $" Current Streak: {current} \n Best streak: {best}\n{status}";

        if (doneToday)
        {
            label.color = new Color(0.25f, 0.9f, 0.4f);
        }
        else if (secs <= 3600)
        {
            label.color = new Color(0.95f, 0.35f, 0.35f);
        }
        else
        {
            label.color = Color.black;
        }
    }
}