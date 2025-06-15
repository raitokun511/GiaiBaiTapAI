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
    
    private string prompt = "Explain how a simple neural network works.";
    private string responseText = "";


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

        using (UnityWebRequest www = new UnityWebRequest("xapiUrl + xapiKey", "POST"))
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