using System;

[Serializable]
public class AnalyticsEvent
{
    public string student_id;
    public string event_type;     // e.g. "session_start", "lesson_passed", "question_answered"
    public string topic;          // e.g. "laplace"
    public string question_id;    // e.g. "LAP_Q12"
    public int correct;           // 1/0
    public int attempts;          // number of tries
    public float time_spent_sec;  // seconds on this action
    public string client_time;    // ISO timestamp
}
