using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using System.Text;
using System.IO;
using System.Collections;
using UnityEngine.Analytics;

#if UNITY_EDITOR
public class ChooseUploadFile : EditorWindow
{
    private Texture2D selectedTexture;
    private string apiKey = "AIzaSyBXY3c3kFAHgsPsbjWJNNC014zH602lsW0";
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=";
    private static string xapiKey = "AIzaSyBXY3c3kFAHgsPsbjWJNNC014zH602lsW0";
    private static string xapiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=";
    private string captionResult = "";
    private string prompt = "Explain how a simple neural network works.";
    private string responseText = "";

    [MenuItem("Window/Gemini Image Caption")]
    public static void ShowWindow()
    {
        GetWindow<ChooseUploadFile>("Gemini Caption");//5890915110
    }

    void OnGUI()
    {
        GUILayout.Label("Select Image", EditorStyles.boldLabel);
        selectedTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", selectedTexture, typeof(Texture2D), false);

        if (GUILayout.Button("Send Image to Gemini"))
        {
            if (selectedTexture != null)
            {
                captionResult = "Sending...";
                Repaint();
                SendImageToGemini(selectedTexture);
                //SendTextToGemini();
            }
            else
            {
                captionResult = "Please select an image.";
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Caption Result:", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(captionResult, GUILayout.Height(100));
    }

    async void SendTextToGemini()
    {
        string jsonData = $@"{{
          ""contents"": [{{
            ""parts"": [{{""text"": ""{prompt}""}}]
          }}]
        }}";

        using (UnityWebRequest www = new UnityWebRequest(apiUrl + apiKey, "POST"))
        {
            Debug.Log("Call API: " + apiUrl + apiKey);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            Debug.Log("Body:" + www.ToString());

            var asyncOperation = www.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield(); // Hoặc Task.Delay(TimeSpan.FromMilliseconds(someValue));
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Gemini Response: " + www.downloadHandler.text);
                try
                {
                    GeminiResponse textResponse = JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text);
                    if (textResponse?.candidates?[0]?.content?.parts?[0]?.text != null)
                    {
                        responseText = textResponse.candidates[0].content.parts[0].text;
                    }
                    else
                    {
                        responseText = "Could not parse text response.";
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing Gemini response: " + e.Message);
                    responseText = "Error parsing response.";
                }
            }
            else
            {
                Debug.LogError("Gemini API request failed: " + www.error);
                responseText = "API request failed: " + www.error;
            }

            Repaint();
        }
    }

    async void SendImageToGemini(Texture2D texture)
    {
        byte[] imageBytes = texture.EncodeToJPG(); // You can choose PNG as well
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

        using (UnityWebRequest www = new UnityWebRequest(apiUrl + apiKey, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            var asyncOperation = www.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield(); // Hoặc Task.Delay(TimeSpan.FromMilliseconds(someValue));
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Gemini Response: " + www.downloadHandler.text);
                // Parse the JSON response to extract the text
                try
                {
                    JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text);
                    if (JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text).candidates != null &&
                        JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text).candidates.Length > 0 &&
                        JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text).candidates[0].content != null &&
                        JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text).candidates[0].content.parts != null &&
                        JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text).candidates[0].content.parts.Length > 0)
                    {
                        captionResult = JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text).candidates[0].content.parts[0].text;
                    }
                    else
                    {
                        captionResult = "Could not parse caption from response.";
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing Gemini response: " + e.Message);
                    captionResult = "Error parsing response.";
                }
            }
            else
            {
                Debug.LogError("Gemini API request failed: " + www.error);
                captionResult = "API request failed: " + www.error;
                captionResult += "\n" + bodyRaw.ToString();
            }

            Repaint();
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
public class EditorGeminiResponse
{
    public Candidate[] candidates;
}

[Serializable]
public class EditorCandidate
{
    public Content content;
}

[Serializable]
public class EditorContent
{
    public Part[] parts;
}

[Serializable]
public class EditorPart
{
    public string text;
}
#endif