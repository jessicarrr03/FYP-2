using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using UnityEngine.Events;

public class ChatGPTManager : MonoBehaviour
{
    public OnResponseEvent OnResponse; 

    [System.Serializable]

    public class OnResponseEvent : UnityEvent<string> { }
    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();

    public async void AskChatGPT(string newText)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText; 
        newMessage.Role = "user";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-4o-mini";
        request.MaxTokens = 200;
        request.Temperature = 0.4f;

        var response = await openAI.CreateChatCompletion(request);

        if(response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            Debug.Log(chatResponse.Content); 

            OnResponse.Invoke(chatResponse.Content);
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        messages.Add(new ChatMessage
        {
            Role = "system",
            Content =
            "You are an EE2101 Circuit Analysis tutor. " +
            "Explain at most two steps per reply. " +
            "Keep explanations concise and mathematical. " +
            "End every reply with a question that checks understanding or asks the user to type 'next' to continue. " +
            "Do not give the full solution immediately."
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
