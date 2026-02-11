using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudentIdGate : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField studentIdInput;
    [SerializeField] private TMP_Text errorText;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "1 StartingScene"; 

    private void Start()
{
    if (errorText) errorText.text = "";

    // Optional: prefill input if an ID already exists
    if (StudentIdentity.HasId())
        studentIdInput.text = StudentIdentity.GetId();
}

public void OnClickStart()
{
    var id = studentIdInput.text.Trim();

    if (string.IsNullOrEmpty(id))
    {
        if (errorText) errorText.text = "Please enter your NTU student ID.";
        return;
    }

    StudentIdentity.SetId(id);

    // Log a check-in row immediately (so CSV has data)
    if (AnalyticsClient.I == null)
    {
        Debug.LogError("[Login] AnalyticsClient.I is NULL. Did you add AnalyticsClient to the login scene?");
    }
    else
    {
        AnalyticsClient.I.Log(new AnalyticsEvent
        {
            student_id = StudentIdentity.GetId(),
            event_type = "check_in",
            topic = "",
            question_id = "",
            correct = 0,
            attempts = 0,
            time_spent_sec = 0
        });

        Debug.Log("[Login] Logged check_in for " + StudentIdentity.GetId());
    }

    if (StreakManager.I != null)
    StreakManager.I.CheckInToday();

    SceneManager.LoadScene(nextSceneName);
}

}
