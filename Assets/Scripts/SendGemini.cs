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