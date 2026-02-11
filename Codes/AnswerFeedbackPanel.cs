using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnswerFeedbackPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button okButton;

    private Action _onClose;

    void Awake()
    {
        if (okButton != null) okButton.onClick.AddListener(HandleOk);
    }

    public void Show(bool isCorrect, Action onClose)
    {
        _onClose = onClose;
        if (messageText != null)
        {
            messageText.text = isCorrect ? " Correct!" : " Incorrect!";
            messageText.color = isCorrect ? new Color(0.2f, 0.8f, 0.3f) : new Color(0.9f, 0.3f, 0.3f);
        }
        gameObject.SetActive(true);
    }

    private void HandleOk()
    {
        gameObject.SetActive(false);
        _onClose?.Invoke();
        _onClose = null;
    }
}
