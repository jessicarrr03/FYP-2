using TMPro;
using UnityEngine;

public class StageTitleUI : MonoBehaviour
{
    [SerializeField] TMP_Text title;

    void Start()
    {
        var nameToShow = GameState.I?.CurrentStageName ?? "Stage";
        title.text = nameToShow;
    }
}