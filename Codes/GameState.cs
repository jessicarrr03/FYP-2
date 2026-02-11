using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState I { get; private set; }

    public string CurrentStageId { get; private set; }
    public string CurrentStageName { get; private set; }

    void Awake()
    {
        if (I != this && I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetStage(string id, string displayName = null)
    {
        CurrentStageId = id;
        CurrentStageName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
    }
}
