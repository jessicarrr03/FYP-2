using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class NALAManager : MonoBehaviour
{
    public OnResponseEvent OnResponse;

    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string> { }

    [Header("NALA Settings")]
    [SerializeField] private string baseUrl = "https://nala.ntu.edu.sg";
    [SerializeField] private string apiKey = "pk_LOKE0085_afadsf43thetyeuewta"; // DEV ONLY; do not ship in build
    [SerializeField] private string llmPath = "/api/llm/";

    [Header("LLM Settings")]
    [SerializeField] private string model = "gpt-5";
    [SerializeField] private int maxOutputTokens = 200;
    [SerializeField] private float temperature = 0.4f;

    [Header("Tutor Prompt")]
    [TextArea(3, 8)]
    [SerializeField] private string systemPrompt =
        "You are an EE2101 Circuit Analysis tutor. " +
        "Explain at most two steps per reply. " +
        "Keep explanations concise and mathematical. " +
        "End every reply with a question that checks understanding or asks the user to type 'next' to continue. " +
        "Do not give the full solution immediately.";

   
    public async void AskNALA(string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
            return;

        string xmlBody = BuildLlmXml(
            model: model,
            systemPrompt: systemPrompt,
            userPrompt: newText,
            temperature: temperature,
            maxOutputTokens: maxOutputTokens
        );

        try
        {
            string json = await PostXmlAsync($"{baseUrl}{llmPath}", apiKey, xmlBody);
            string reply = ExtractAssistantText(json);

            Debug.Log(reply);
            OnResponse?.Invoke(reply);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
            OnResponse?.Invoke("Error contacting NALA. Please try again.");
        }
    }

    // 1) Make the XML body
    private static string BuildLlmXml(string model, string systemPrompt, string userPrompt, float temperature, int maxOutputTokens)
    {
        string Esc(string s) => XmlEscape(s);

        return
        
        $@"<llm_request>
        <model>{Esc(model)}</model>
        <system_prompt>{Esc(systemPrompt)}</system_prompt>
        <user_prompt>{Esc(userPrompt)}</user_prompt>
        </llm_request>";
    }

    private static string XmlEscape(string s)
    {
        if (s == null) return "";
        return s.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
    }

    // Send request, get JSON response
    private static async Task<string> PostXmlAsync(string url, string apiKey, string xmlBody)
    {
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(xmlBody));
        req.downloadHandler = new DownloadHandlerBuffer();

        var key = apiKey.Trim();
        req.SetRequestHeader("Authorization", "Bearer " + key);
        req.SetRequestHeader("Content-Type", "application/xml");
        req.SetRequestHeader("Accept", "application/json");

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
            throw new System.Exception($"HTTP {req.responseCode}: {req.error}\n{req.downloadHandler.text}");

        return req.downloadHandler.text;
    }

    // Parse response: pull assistant text from output[]
    private static string ExtractAssistantText(string json)
    
    {
    var root = JObject.Parse(json);

    // NALA wraps the LLM response under "raw"
    var output = root["raw"]?["output"] as JArray;
    if (output == null) return json;

    foreach (var item in output)
    {
        if ((string)item["type"] == "message" && (string)item["role"] == "assistant")
        {
            var contentArr = item["content"] as JArray;
            if (contentArr == null) continue;

            var sb = new StringBuilder();
            foreach (var c in contentArr)
            {
                if ((string)c["type"] == "output_text")
                    sb.Append((string)c["text"]);
            }

            var text = sb.ToString().Trim();
            if (!string.IsNullOrEmpty(text))
                return text;
        }
    }

    return json; // fallback
    }

}
