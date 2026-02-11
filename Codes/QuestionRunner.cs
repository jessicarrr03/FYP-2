using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionRunner : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text stageTitle;
    [SerializeField] TMP_Text questionText;
    [SerializeField] Button btnA, btnB, btnC, btnD;
    [SerializeField] TMP_Text txtA, txtB, txtC, txtD;
    [SerializeField] Image questionImage;
    [SerializeField] GameObject resultsPanel;
    [SerializeField] TMP_Text resultsText;
    [SerializeField] string mapSceneName = "Week1Map";
    [SerializeField] AnswerFeedbackPanel feedbackPanel;
    [SerializeField] GameObject answersGroup;

    const int QUESTIONS_TO_ASK = 15;
    const int PASS_MARK = 1;

    // Optional counters (useful if you later add a "quiz_summary" analytics event)
    int correctCount = 0;
    int wrongCount = 0;

    int attemptsThisQuestion = 0;
    string currentQuestionId = "";

    List<Question> selected;
    int index = 0;
    int score = 0;

    float questionStartTime;

    readonly Dictionary<string, Sprite> spriteCache = new();

    string GetStableQuestionId(Question q)
    {
        // Preferred: from your questions CSV via QuestionLoader -> Question.QuestionId
        if (q != null && !string.IsNullOrEmpty(q.QuestionId))
            return q.QuestionId;

        // Fallback: stage + index (stable within this run)
        return $"{GameState.I?.CurrentStageId}_Q{index + 1}";
    }

    void Start()
    {
        stageTitle.text = GameState.I?.CurrentStageName ?? GameState.I?.CurrentStageId ?? "Stage";

        var loader = FindObjectOfType<QuestionLoader>();
        var all = loader.AllForStage;
        selected = PickUniqueRandom(all, QUESTIONS_TO_ASK);

        WireButtons();
        ShowCurrent();
    }

    void WireButtons()
    {
        btnA.onClick.AddListener(() => Answer('A'));
        btnB.onClick.AddListener(() => Answer('B'));
        btnC.onClick.AddListener(() => Answer('C'));
        btnD.onClick.AddListener(() => Answer('D'));
    }

    static List<Question> PickUniqueRandom(List<Question> source, int take)
    {
        var list = new List<Question>(source);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        if (take > list.Count) take = list.Count;
        return list.GetRange(0, take);
    }

    void SetAnswersInteractable(bool on)
    {
        if (answersGroup == null)
        {
            if (btnA) btnA.interactable = on;
            if (btnB) btnB.interactable = on;
            if (btnC) btnC.interactable = on;
            if (btnD) btnD.interactable = on;
            return;
        }

        foreach (var b in answersGroup.GetComponentsInChildren<Button>(true))
            b.interactable = on;
    }

    void ShowCurrent()
    {
        if (index >= selected.Count) { Finish(); return; }

        var q = selected[index];

        questionText.text = $"{index + 1}/{selected.Count}  {q.Text}";
        txtA.text = q.A; txtB.text = q.B; txtC.text = q.C; txtD.text = q.D;

        if (questionImage != null)
        {
            if (!string.IsNullOrEmpty(q.ImageKey))
            {
                if (!spriteCache.TryGetValue(q.ImageKey, out var sp))
                {
                    sp = Resources.Load<Sprite>(q.ImageKey);
                    if (sp != null) spriteCache[q.ImageKey] = sp;
                }

                questionImage.sprite = sp;
                questionImage.enabled = (sp != null);
                questionImage.preserveAspect = true;
            }
            else
            {
                questionImage.sprite = null;
                questionImage.enabled = false;
            }
        }

        questionStartTime = Time.realtimeSinceStartup;

        // reset per-question tracking
        attemptsThisQuestion = 0;
        currentQuestionId = GetStableQuestionId(q);
    }

    void Answer(char pick)
    {
        if (index >= selected.Count) return;

        var q = selected[index];

        // per-question attempt count (if you currently lock answers after 1 click, this will stay at 1)
        attemptsThisQuestion++;

        bool isCorrect = char.ToUpperInvariant(pick) == q.Correct;

        if (isCorrect)
        {
            score++;
            correctCount++;
        }
        else
        {
            wrongCount++;
        }

        // time spent on this question
        float timeSpent = Time.realtimeSinceStartup - questionStartTime;

        // prefer currentQuestionId (already has fallback), but keep an extra fallback just in case
        string qid = currentQuestionId;
        if (string.IsNullOrEmpty(qid))
            qid = (q != null) ? q.QuestionId : "";
        if (string.IsNullOrEmpty(qid))
            qid = $"{GameState.I?.CurrentStageId}_Q{index + 1}";

        // post/log event
        if (AnalyticsClient.I != null && StudentIdentity.HasId())
        {
            AnalyticsClient.I.Log(new AnalyticsEvent
            {
                student_id = StudentIdentity.GetId(),
                event_type = "question_answered",
                topic = GameState.I?.CurrentStageName ?? GameState.I?.CurrentStageId ?? "",
                question_id = qid,
                correct = isCorrect ? 1 : 0,
                attempts = attemptsThisQuestion,
                time_spent_sec = timeSpent
            });
        }

        SetAnswersInteractable(false);

        if (feedbackPanel != null)
        {
            feedbackPanel.Show(isCorrect, onClose: () =>
            {
                index++;
                if (index >= selected.Count)
                {
                    Finish();
                }
                else
                {
                    ShowCurrent();
                    SetAnswersInteractable(true);
                }
            });
        }
        else
        {
            index++;
            if (index >= selected.Count) Finish();
            else { ShowCurrent(); SetAnswersInteractable(true); }
        }
    }

    void Finish()
    {
        bool passed = score >= PASS_MARK;
        resultsPanel.SetActive(true);
        resultsText.text =
            $"Score: {score}/{selected.Count}\n" +
            (passed ? "PASSED!! Please head to the next stage!" : "Try again");

        StageProgress.SaveStageResult(GameState.I.CurrentStageId, score, selected.Count, passed);

        if (passed) StreakManager.I?.OnLessonPassedToday();
    }

    System.Collections.IEnumerator ReturnToMapAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        UnityEngine.SceneManagement.SceneManager.LoadScene(mapSceneName);
    }
}
