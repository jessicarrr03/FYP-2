using UnityEngine;

public class SessionTimer : MonoBehaviour
{
    private float startTime;

    void Start()
    {
        startTime = Time.time;

        if (AnalyticsClient.I != null && StudentIdentity.HasId())
        {
            AnalyticsClient.I.Log(new AnalyticsEvent
            {
                student_id = StudentIdentity.GetId(),
                event_type = "session_start",
                topic = "",
                question_id = "",
                correct = 0,
                attempts = 0,
                time_spent_sec = 0
            });
        }
    }

    void OnApplicationQuit()
    {
        if (AnalyticsClient.I != null && StudentIdentity.HasId())
        {
            AnalyticsClient.I.Log(new AnalyticsEvent
            {
                student_id = StudentIdentity.GetId(),
                event_type = "session_end",
                topic = "",
                question_id = "",
                correct = 0,
                attempts = 0,
                time_spent_sec = Time.time - startTime
            });
        }
    }
}
