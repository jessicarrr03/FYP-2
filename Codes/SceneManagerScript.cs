using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject streakLostPopup;
    [SerializeField] private GameObject loadingPanel;

    private void Start()
    {

        // ✅ Count entering StartingScene as a daily check-in
        if (StreakManager.I != null)
        {
            StreakManager.I.CheckInToday(); // only if you added this method earlier
        
        }


        if (StreakManager.I != null && StreakManager.I.lostStreakToday)
        {
            if (streakLostPopup != null)
            {
                streakLostPopup.SetActive(true);
            }
            StreakManager.I.lostStreakToday = false;
        }
    }

    public void LoadScene(string sceneName)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        SceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void ClosePopup(GameObject popup)
    {
        popup.SetActive(false);
    }

    public void CloseStreakPopup()
    {
        if (streakLostPopup != null)
            streakLostPopup.SetActive(false);
    }
}