using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SolveController : MonoBehaviour
{
    public static SolveController instance;

    [SerializeField]
    WebCamFullScreenCover WebCamFullScreenCover;
    [SerializeField]
    RectTransform webcamRectTransform;

    public TMP_InputField input;
    public Text outputText;
    public RectTransform outputContent;

    public Button submitButton;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendImageProblem(RawImage rawImage)
    {
        if (!rawImage.texture.isReadable)
        {
            Debug.Log("Texture not read");
            //outputText.text += "\nTexture not read";
            RenderTexture tempRT = new RenderTexture(rawImage.texture.width, rawImage.texture.height, 0);
            Graphics.Blit(rawImage.texture, tempRT);
            // Kích hoạt RenderTexture
            RenderTexture.active = tempRT;
            // Tạo Texture2D mới
            Texture2D newTexture = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGBA32, false);
            newTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            newTexture.Apply();

            RenderTexture.active = null;
            tempRT.Release();

            ProcessImage(newTexture);
        }
        else
        {
            RectTransform imageCropperRT = rawImage.rectTransform;
            float width = imageCropperRT.rect.width;
            float height = imageCropperRT.rect.height;
            Texture2D newTexture = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
            newTexture.SetPixels32(newTexture.GetPixels32());
            newTexture.Apply();
            //outputText.text += "\nNew Texture:" + rawImage.texture.width + "//" + rawImage.texture.height;

            ProcessImage(newTexture);
            CloseCamera();
        }
    }
    public void ProcessImage(Texture2D imageToSend)
    {
        string requestPrompt = StringConst.IMAGE_SEND_REQUEST;
        if (imageToSend != null)
        {

            StartCoroutine(TestAPI.instance.GetSendImageToGemini(imageToSend, requestPrompt, (response) =>
            {
                //Giải bài toán : Cho 0.5 mol HCl tác dụng 0.4 mol NaOH, tính khối lượng muốn sinh ra
                outputText.text = response + "\n" + "\n" + "\n" + "\n" + "\n" + "\n" + "\n" + "\n";
                LayoutRebuilder.ForceRebuildLayoutImmediate(outputText.GetComponent<RectTransform>()); // Đảm bảo kích thước được cập nhật ngay lập tức

                RectTransform outputRect = outputText.GetComponent<RectTransform>();
                Debug.Log("Submit Response:" + (-outputRect.rect.height / 2f));
                outputRect.anchoredPosition = new Vector2(outputRect.anchoredPosition.x, -outputRect.rect.height / 2f);
                outputContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, outputRect.rect.width);
                outputContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, outputRect.rect.height);

            }));

            /*
            StartCoroutine(SendGemini.GetSendImageToGemini(imageToSend, (caption) =>
            {
                if (!string.IsNullOrEmpty(caption))
                {
                    Debug.Log("Gemini Result: " + caption);
                    if (outputText != null)
                    {
                        outputText.text = caption;
                    }
                }
                else
                {
                    Debug.LogError("Failed to get result from Gemini.");
                }
            }));
            */
        }
        else
        {
            Debug.LogError("Image to send is not assigned.");
        }
    }

    public void SubmitProblem()
    {
        Debug.Log("Submit Click " + input.text.Trim());
        if (input != null && input.text.Trim().Length > 0)
        {
            Debug.Log("Submit: " + input.text);

            StartCoroutine(TestAPI.instance.SendMessageToGemini(input.text, (response) =>
            {
                //Giải bài toán : Cho 0.5 mol HCl tác dụng 0.4 mol NaOH, tính khối lượng muốn sinh ra
                outputText.text = response + "\n" + "\n" + "\n" + "\n" + "\n" + "\n" + "\n" + "\n";
                LayoutRebuilder.ForceRebuildLayoutImmediate(outputText.GetComponent<RectTransform>()); // Đảm bảo kích thước được cập nhật ngay lập tức

                RectTransform outputRect = outputText.GetComponent<RectTransform>();
                Debug.Log("Submit Response:" + (-outputRect.rect.height / 2f));
                outputRect.anchoredPosition = new Vector2(outputRect.anchoredPosition.x, -outputRect.rect.height / 2f);
                outputContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, outputRect.rect.width);
                outputContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, outputRect.rect.height);

            }));
            /*
            StartCoroutine(SendGemini.SendPromptToGemini(input.text, (response) =>
            {
                //Giải bài toán : Cho 0.5 mol HCl tác dụng 0.4 mol NaOH, tính khối lượng muốn sinh ra
                outputText.text = response + "\n" + "\n" + "\n" + "\n" + "\n" + "\n" + "\n" + "\n";
                LayoutRebuilder.ForceRebuildLayoutImmediate(outputText.GetComponent<RectTransform>()); // Đảm bảo kích thước được cập nhật ngay lập tức

                RectTransform outputRect = outputText.GetComponent<RectTransform>();
                Debug.Log("Submit Response:" + (-outputRect.rect.height / 2f));
                outputRect.anchoredPosition = new Vector2(outputRect.anchoredPosition.x, -outputRect.rect.height / 2f);
                outputContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, outputRect.rect.width);
                outputContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, outputRect.rect.height);

            }));
            */
        }
    }

    public void OpenCamera()
    {
        webcamRectTransform.gameObject.SetActive(true);
        if (WebCamFullScreenCover != null)
        {
            //WebCamFullScreenCover.StartCamera();
        }
    }
    public void CloseCamera()
    {
        WebCamFullScreenCover.CloseWebcam();
        webcamRectTransform.gameObject.SetActive(false);
    }
}
