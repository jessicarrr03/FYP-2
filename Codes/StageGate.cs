using UnityEngine;
using UnityEngine.UI;

public class StageGate : MonoBehaviour
{
    [SerializeField] string prerequisiteStageId;
    [SerializeField] Button targetButton;
    [SerializeField] GameObject lockedOverlay;

    void Awake()
    {
        if (targetButton == null) targetButton = GetComponent<Button>();
    }

    void OnEnable()
    {
        Invoke(nameof(Refresh), 0.05f);
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = StageProgress.IsStagePassed(prerequisiteStageId);

        if (targetButton != null)
            targetButton.interactable = unlocked;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!unlocked);

        Debug.Log($"[StageGate] prereq={prerequisiteStageId} everPassed={unlocked} -> button.interactable={unlocked}");
    }
}