using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using System.Text;
using System.IO;
using System.Collections;
using UnityEngine.Analytics;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;

public class SendGemini : MonoBehaviour
{
    private Texture2D selectedTexture;
    private string apiKey = "XXX";
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=";
    private static string xapiKey = "XXX";
    private static string xapiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=";
    private string captionResult = "";
    private string prompt = "Explain how a simple neural network works.";
    private string responseText = "";


    public static IEnumerator SendPromptToGemini(string promptText, Action<string> onComplete)
    {
        if (string.IsNullOrEmpty(xapiKey))
        {
            Debug.LogError("API Key Gemini chưa được thiết lập!");
            onComplete(null);
            yield break;
        }

        string url = $"{xapiUrl}{xapiKey}";
        Debug.Log("Send URL:" +  url);

        GeminiRequest requestData = new GeminiRequest
        {
            contents = new Content[]
            {
                new Content
                {
                    parts = new Part[]
                    {
                        new Part { text = promptText }
                    }
                }
            }
        };

        string jsonPayload = JsonConvert.SerializeObject(requestData);

        using (UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            Debug.Log("Send WWW");
            yield return www.SendWebRequest();

            string result = null;
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Lỗi khi gửi prompt đến Gemini: {www.error}");
                Debug.LogError($"Response Code: {www.responseCode}");
                Debug.LogError($"Response Body: {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Phản hồi từ Gemini: {www.downloadHandler.text}");
                GeminiResponse response = JsonConvert.DeserializeObject<GeminiResponse>(www.downloadHandler.text);
                if (response?.candidates != null && response.candidates.Length > 0 && response.candidates[0].content.parts != null && response.candidates[0].content.parts.Length > 0)
                {
                    result = response.candidates[0].content.parts[0].text;
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy kết quả hợp lệ trong phản hồi từ Gemini.");
                    Debug.Log($"Full Response: {www.downloadHandler.text}");
                }
            }
            onComplete?.Invoke(result);

        }
    }
    public static IEnumerator GetSendImageToGemini(Texture2D texture, System.Action<string> onComplete)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null.");
            onComplete?.Invoke(null);
            yield break;
        }

        byte[] imageBytes = texture.EncodeToJPG();
        string base64Image = Convert.ToBase64String(imageBytes);

        string jsonData = $@"{{
          ""contents"": [
            {{
              ""parts"": [
                {{
                  ""inline_data"": {{
                    ""mime_type"": ""image/jpeg"",
                    ""data"": ""{base64Image}""
                  }}
                }},
                {{
                  ""text"": ""Hãy giải bài toán hóa học trong ảnh sau:""
                }}
              ]
            }}
          ]
        }}";

        using (UnityWebRequest www = new UnityWebRequest(xapiUrl + xapiKey, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            string result = null;
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Gemini Response: " + www.downloadHandler.text);
                try
                {
                    GeminiResponse responseData = JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text);
                    result = responseData?.candidates?[0]?.content?.parts?[0]?.text;
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing Gemini response: " + e.Message);
                    result = "Error parsing response.";
                }
            }
            else
            {
                Debug.LogError("Gemini API request failed: " + www.error);
                result = "API request failed: " + www.error + "\n" + Encoding.UTF8.GetString(bodyRaw);
            }
            onComplete?.Invoke(result);
        }
    }
}

[Serializable]
public struct GeminiRequest
{
    public Content[] contents;
}
[Serializable]
public class GeminiResponse
{
    public Candidate[] candidates;
}

[Serializable]
public class Candidate
{
    public Content content;
}

[Serializable]
public class Content
{
    public Part[] parts;
}

[Serializable]
public class Part
{
    public string text;
}