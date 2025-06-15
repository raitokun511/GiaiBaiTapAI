using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class CallAIRequest : MonoBehaviour
{

    public static CallAIRequest instance;
    //NewGeminiAPI
    //"https://script.google.com/macros/s/AKfycbxuIDUhngfcjgeMv07NGI7A69beJw2ABnRfL-cVAK78BGPsS5dAgVpGfs_7T3jTDN63cg/exec"

    //GeminiImageChat
    public string imageScript = "https://script.google.com/macros/s/AKfycbzUnt48VrlwwmrtZ6nPLyVJjDkOR5JLrtXrApcg38282OCAM-3NnlHiX1-JvvtzhGhM/exec";


    // Kéo và thả Web App URL từ Apps Script vào đây trong Inspector
    [SerializeField] private string appsScriptWebAppUrl = "YOUR_WEB_APP_URL_HERE";
    public Texture2D testTexture;

    // Cấu trúc dữ liệu cho lịch sử chat (tương tự như trong Apps Script)
    [System.Serializable]
    public class Part
    {
        public string text;
    }

    [System.Serializable]
    public class Content
    {
        public string role;
        public Part[] parts;
    }

    // Cấu trúc request payload gửi tới Apps Script
    [System.Serializable]
    public class ChatRequestPayload
    {
        public string userMessage;
        public Content[] chatHistory; // Sử dụng mảng Content
        public string base64Image;
        public string mimeType;
    }

    // Cấu trúc response nhận từ Apps Script
    [System.Serializable]
    public class ChatResponsePayload
    {
        public string response; // Câu trả lời từ Gemini
        public string error;    // Nếu có lỗi từ Apps Script
    }

    [System.Serializable]
    public class ImageChatRequestPayload
    {
        public string userMessage;
        public string base64Image;
        public string mimeType;
    }

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator SendMessageToGemini(string message, Action<string> callback, Content[] history = null)
    {
        // Đảm bảo URL đã được thiết lập
        if (string.IsNullOrEmpty(appsScriptWebAppUrl) || appsScriptWebAppUrl == "YOUR_WEB_APP_URL_HERE")
        {
            Debug.LogError("Apps Script Web App URL chưa được thiết lập trong Inspector!");
            callback?.Invoke("Lỗi: URL chưa được cấu hình."); // Gọi callback với lỗi
            yield break; // Thoát Coroutine
        }

        // Tạo payload
        ChatRequestPayload requestPayload = new ChatRequestPayload
        {
            userMessage = message,
            chatHistory = history ?? new Content[0] // Nếu lịch sử trống, gửi mảng rỗng
        };

        string jsonPayload = JsonUtility.ToJson(requestPayload);
        Debug.Log("Sending JSON: " + jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(appsScriptWebAppUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Gửi yêu cầu và chờ phản hồi
            // Sử dụng yield return để đợi SendWebRequest() hoàn tất mà không chặn Main Thread
            yield return request.SendWebRequest(); // Đây là nơi thay thế 'await operation;'

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
                callback?.Invoke("Lỗi kết nối: " + request.error); // Gọi callback với lỗi
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Received: " + responseText);

                try
                {
                    ChatResponsePayload response = JsonUtility.FromJson<ChatResponsePayload>(responseText);

                    if (!string.IsNullOrEmpty(response.error))
                    {
                        Debug.LogError("Apps Script Error: " + response.error);
                        callback?.Invoke("Lỗi từ Apps Script: " + response.error);
                    }
                    else if (!string.IsNullOrEmpty(response.response))
                    {
                        callback?.Invoke(response.response); // Trả về câu trả lời của Gemini qua callback
                    }
                    else
                    {
                        Debug.LogError("Unexpected response format: " + responseText);
                        callback?.Invoke("Lỗi: Định dạng phản hồi không mong muốn.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to parse JSON response: " + e.Message + " Response: " + responseText);
                    callback?.Invoke("Lỗi: Không thể phân tích phản hồi.");
                }
            }
        }
    }

    // Hàm public để gọi từ các script khác
    public async Task<string> SendMessageToGemini(string message, Content[] history = null)
    {
        // Đảm bảo URL đã được thiết lập
        if (string.IsNullOrEmpty(appsScriptWebAppUrl) || appsScriptWebAppUrl == "YOUR_WEB_APP_URL_HERE")
        {
            Debug.LogError("Apps Script Web App URL chưa được thiết lập trong Inspector!");
            return "Lỗi: URL chưa được cấu hình.";
        }

        // Tạo payload
        ChatRequestPayload requestPayload = new ChatRequestPayload
        {
            userMessage = message,
            chatHistory = history ?? new Content[0] // Nếu lịch sử trống, gửi mảng rỗng
        };

        string jsonPayload = JsonUtility.ToJson(requestPayload);
        Debug.Log("Sending JSON: " + jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(appsScriptWebAppUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Gửi yêu cầu và chờ phản hồi
            // Use SendWebRequest() which returns an AsyncOperation
            var operation = request.SendWebRequest();

            // Chờ cho đến khi yêu cầu hoàn tất
            while (!operation.isDone)
            {
                await Task.Yield(); // Tạm dừng thực thi để không chặn Main Thread
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
                return "Lỗi kết nối: " + request.error;
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Received: " + responseText);

                try
                {
                    ChatResponsePayload response = JsonUtility.FromJson<ChatResponsePayload>(responseText);

                    if (!string.IsNullOrEmpty(response.error))
                    {
                        Debug.LogError("Apps Script Error: " + response.error);
                        return "Lỗi từ Apps Script: " + response.error;
                    }
                    else if (!string.IsNullOrEmpty(response.response))
                    {
                        return response.response; // Trả về câu trả lời của Gemini
                    }
                    else
                    {
                        Debug.LogError("Unexpected response format: " + responseText);
                        return "Lỗi: Định dạng phản hồi không mong muốn.";
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to parse JSON response: " + e.Message + " Response: " + responseText);
                    return "Lỗi: Không thể phân tích phản hồi.";
                }
            }
        }
    }

    public IEnumerator GetSendImageToGemini(Texture2D texture, string userQuestion, System.Action<string> onComplete)
    {
        if (string.IsNullOrEmpty(appsScriptWebAppUrl) || appsScriptWebAppUrl == "YOUR_WEB_APP_URL_HERE")
        {
            Debug.LogError("Apps Script Web App URL chưa được thiết lập trong Inspector!");
            onComplete?.Invoke("Lỗi: URL chưa được cấu hình.");
            yield break; // Kết thúc IEnumerator
        }

        if (texture == null)
        {
            Debug.LogError("Texture2D không được cung cấp!");
            onComplete?.Invoke("Lỗi: Không có hình ảnh để gửi.");
            yield break;
        }

        // Chuyển Texture2D sang Base64
        // Tùy chọn: Chọn định dạng ảnh (PNG thường tốt cho ảnh có độ trong suốt)
        byte[] imageBytes;
        string mimeType;

        // Ưu tiên PNG nếu không có yêu cầu đặc biệt về JPG
        imageBytes = texture.EncodeToJPG();
        mimeType = "image/jpeg";

        // Hoặc dùng JPEG nếu bạn muốn kích thước file nhỏ hơn và không cần trong suốt
        // imageBytes = texture.EncodeToJPG();
        // mimeType = "image/jpeg";


        string base64Image = Convert.ToBase64String(imageBytes);
        Debug.Log("Image Base64 length: " + base64Image.Length);

        // Tạo payload JSON
        ImageChatRequestPayload requestPayload = new ImageChatRequestPayload
        {
            userMessage = userQuestion,
            base64Image = base64Image,
            mimeType = mimeType
        };

        string jsonPayload = JsonUtility.ToJson(requestPayload);
        Debug.Log("Sending image JSON: " + jsonPayload.Substring(0, Mathf.Min(jsonPayload.Length, 200)) + "..."); // Chỉ log vài ký tự đầu

        using (UnityWebRequest request = new UnityWebRequest(appsScriptWebAppUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Gửi yêu cầu
            yield return request.SendWebRequest(); // Chờ yêu cầu hoàn thành

            string finalResponse = "";

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error sending image to Apps Script: " + request.error);
                finalResponse = "Lỗi kết nối: " + request.error;
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Received image response: " + responseText);

                try
                {
                    ChatResponsePayload response = JsonUtility.FromJson<ChatResponsePayload>(responseText);

                    if (!string.IsNullOrEmpty(response.error))
                    {
                        finalResponse = "Lỗi từ Apps Script: " + response.error;
                        Debug.LogError(finalResponse);
                    }
                    else if (!string.IsNullOrEmpty(response.response))
                    {
                        // Kiểm tra phản hồi đặc biệt cho trường hợp "NONE_QUESTION"
                        if (response.response == "NONE_QUESTION_RESPONSE")
                        {
                            finalResponse = "NONE_QUESTION"; // Trả về mã lỗi đặc biệt
                            Debug.LogWarning("Apps Script/Gemini: Hình ảnh không được nhận diện là một câu hỏi.");
                        }
                        else
                        {
                            finalResponse = response.response; // Trả về câu trả lời thực tế
                        }
                    }
                    else
                    {
                        finalResponse = "Lỗi: Định dạng phản hồi không mong muốn từ Apps Script.";
                        Debug.LogError(finalResponse + " Response: " + responseText);
                    }
                }
                catch (System.Exception e)
                {
                    finalResponse = "Lỗi: Không thể phân tích phản hồi JSON từ Apps Script: " + e.Message;
                    Debug.LogError(finalResponse + " Response: " + responseText);
                }
            }
            onComplete?.Invoke(finalResponse); // Gọi callback với kết quả cuối cùng
        }
    }


    // --- Hàm ví dụ để gọi từ một MonoBehaviour khác ---
    public async void TestGeminiChatFromUnity(string prompt)
    {
        string testMessage = prompt;
        //"Cho tôi 1 câu hỏi trắc nghiệm ngẫu nhiên về hóa học phổ thông, chủ đề vô cơ - kim loại. cùng trả về 4 đáp án trong đó có 1 đáp án đúng.\r\nCấu trúc bạn trả về dạng json sau và không thêm bất cứ dữ liệu, câu chào nào:\r\n{\r\n\"question\":\"\",\r\n\"right_ans\":\"\",\r\n\"wrong_1\":\"\",\r\n\"wrong_2\":\"\",\r\n\"wrong_3\":\"\"\r\n}";
        Debug.Log("Gửi câu hỏi: " + testMessage);

        string geminiAnswer = await SendMessageToGemini(testMessage);
        Debug.Log("Câu trả lời của Gemini từ Apps Script: " + geminiAnswer);

        // Ví dụ với lịch sử chat
        Content[] chatHistory = new Content[]
        {
            new Content { role = "user", parts = new Part[] { new Part { text = "Chào Gemini!" } } },
            new Content { role = "model", parts = new Part[] { new Part { text = "Chào bạn! Tôi có thể giúp gì cho bạn hôm nay?" } } }
        };
        //string followUpAnswer = await SendMessageToGemini(followUpMessage, chatHistory);
        //Debug.Log("Câu trả lời tiếp theo của Gemini: " + followUpAnswer);
    }

    // Có thể gọi TestGeminiChatFromUnity từ một nút bấm hoặc trong Start()
    void Start()
    {
    }

    public void OnSendText(string sendText)
    {
        TestGeminiChatFromUnity(sendText);
    }

    public void OnSendImage(string sendText)
    {
        appsScriptWebAppUrl = imageScript;
        if (testTexture != null)
        {
            //StartCoroutine(GetSendImageToGemini(testTexture, "Nếu đây là nội dung một bài toán hóa học, hãy giải nó. Nếu không trả về mã lỗi: NONE", (result) =>
            StartCoroutine(GetSendImageToGemini(testTexture, "Hãy giải bài toán hóa học trong ảnh", (result) =>
            {
                Debug.Log("Kết quả từ Gemini (ảnh): " + result);
                if (result == "NONE_QUESTION")
                {
                    Debug.Log("Đây không phải là một câu hỏi trong hình ảnh.");
                }
            }));
        }
        else
        {
            Debug.LogWarning("Hãy gán một Texture2D vào trường 'Test Texture' trong Inspector để thử nghiệm chức năng gửi ảnh.");
        }
    }
}
