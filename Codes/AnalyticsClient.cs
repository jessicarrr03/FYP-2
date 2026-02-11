using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;  // for UnityWebRequest
using System.Collections;      // for IEnumerator

public class AnalyticsClient : MonoBehaviour
{
    public static AnalyticsClient I { get; private set; }

    [Header("NALA")]
    [SerializeField] string nalaEndpoint = "";      // nala to provide: eg. https://<host>/api/
    [SerializeField] string nalaApiKey = "";        // bearer token 
    [SerializeField] bool logDebug = true;          // toggle console logs

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

 
        if (logDebug)
            Debug.Log("[Analytics] AnalyticsClient ready (POST to NALA).");
    }

    public void Log(AnalyticsEvent e)
    {
        if (e == null) return;

        // use UTC ISO8601? 
        if (string.IsNullOrEmpty(e.client_time))
            e.client_time = DateTime.UtcNow.ToString("o"); 

        // start POST coroutine
        if (string.IsNullOrWhiteSpace(nalaEndpoint))
        {
            if (logDebug)
                Debug.LogWarning("[Analytics] NALA endpoint not set. Skipping POST.");
            return;
        }

        StartCoroutine(PostToNala(e)); 
    }

    private IEnumerator PostToNala(AnalyticsEvent e) 
    {
      
        string json = JsonUtility.ToJson(e); 

        using (var req = new UnityWebRequest(nalaEndpoint, "POST")) 
        {
            byte[] body = Encoding.UTF8.GetBytes(json);             
            req.uploadHandler = new UploadHandlerRaw(body);         
            req.downloadHandler = new DownloadHandlerBuffer();      
            req.SetRequestHeader("Content-Type", "application/json");

            
            if (!string.IsNullOrWhiteSpace(nalaApiKey))
                req.SetRequestHeader("Authorization", $"Bearer {nalaApiKey}");

            yield return req.SendWebRequest(); 

            if (req.result != UnityWebRequest.Result.Success)
            {
                if (logDebug)
                    Debug.LogWarning($"[Analytics] NALA POST failed: {req.responseCode} {req.error}");
            }
            else
            {
                if (logDebug)
                    Debug.Log($"[Analytics] NALA POST ok ({req.responseCode})");
            }
        }
    }

    // CSV path accessor removed (moving from csv to server)
}
