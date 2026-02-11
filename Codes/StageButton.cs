using UnityEngine;
using UnityEngine.SceneManagement;

public class StageButton : MonoBehaviour
{
    [SerializeField] string stageId;
    [SerializeField] string stageName;
    [SerializeField] string questionScene = "3 QuestionScene";

    public void OnClick()
    {
        GameState.I.SetStage(stageId, stageName);
        Debug.Log($"[StageButton] Selected stageId={stageId}, stageName={stageName}");
        SceneManager.LoadScene(questionScene);
    }
}