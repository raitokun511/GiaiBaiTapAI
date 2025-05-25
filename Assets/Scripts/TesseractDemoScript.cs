using UnityEngine;
using UnityEngine.UI;
using ImageCropperNamespace;

public class TesseractDemoScript : MonoBehaviour
{
    [SerializeField] private Texture2D imageToRecognize;
    [SerializeField] private Text displayText;
    [SerializeField] private RawImage outputImage;
    private TesseractDriver _tesseractDriver;
    private string _text = "";
    private Texture2D _texture;

    public Text outputText;


    //public RawImage displayImage;
    public Button captureButton;
    public ImageCropperDemo imageCropper;
    private WebCamTexture webcamTexture;

    private void Start()
    {
        //webcamTexture = new WebCamTexture();
        //outputImage.texture = webcamTexture;
        //webcamTexture.Play();

        //_tesseractDriver = new TesseractDriver();
        captureButton.onClick.AddListener(CaptureImage);
        //captureButton.onClick.AddListener(CaptureImageOld);

    }
    
    void ProcessImage(Texture2D imageToSend)
    {
        if (imageToSend != null)
        {
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
        }
        else
        {
            Debug.LogError("Image to send is not assigned.");
        }
    }

    void CaptureImageOld()
    {
        outputImage.gameObject.gameObject.SetActive(true);
        Texture2D texture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false);
        texture.SetPixels32(webcamTexture.GetPixels32());
        texture.Apply();
        outputImage.texture = texture;
        //webcamTexture.Stop();
        outputImage.gameObject.SetActive(false);
        _tesseractDriver = new TesseractDriver();
        Recoginze(texture);
    }

    void CaptureImage()
    {
        //Texture2D texture = new Texture2D(imageToRecognize.width, imageToRecognize.height, TextureFormat.ARGB32, false);
        //Texture2D texture = new Texture2D(imageCropper.webcamTexture.width, imageCropper.webcamTexture.height, TextureFormat.ARGB32, false);
        //RectTransform imageCropperRT = imageCropper.croppedImageHolder.rectTransform;
        //float width = imageCropperRT.rect.width;
        //float height = imageCropperRT.rect.height;

        if (!imageCropper.croppedImageHolder.texture.isReadable)
        {
            Debug.Log("Texture not read");
            outputText.text += "\nTexture not read";
            RenderTexture tempRT = new RenderTexture(imageCropper.croppedImageHolder.texture.width, imageCropper.croppedImageHolder.texture.height, 0);
            Graphics.Blit(imageCropper.croppedImageHolder.texture, tempRT);
            // Kích hoạt RenderTexture
            RenderTexture.active = tempRT;
            // Tạo Texture2D mới
            Texture2D texture = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            texture.Apply();

            RenderTexture.active = null;
            tempRT.Release();

            //_tesseractDriver = new TesseractDriver();
            //Recoginze(texture);

            ProcessImage(texture);
        }
        else
        {
            RectTransform imageCropperRT = imageCropper.croppedImageHolder.rectTransform;
            float width = imageCropperRT.rect.width;
            float height = imageCropperRT.rect.height;
            Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
            texture.SetPixels32(texture.GetPixels32());
            texture.Apply();
            outputText.text += "\nNew Texture:" + texture.width +"//" + texture.height;

            //_tesseractDriver = new TesseractDriver();
            //Recoginze(texture);

            ProcessImage(texture);
        }

    }

    private void Recoginze(Texture2D outputTexture)
    {
        _texture = outputTexture;
        ClearTextDisplay();
        AddToTextDisplay(_tesseractDriver.CheckTessVersion());
        _tesseractDriver.Setup(OnSetupCompleteRecognize);
    }

    private void OnSetupCompleteRecognize()
    {
        AddToTextDisplay("Complete Setup: ");
        AddToTextDisplay(_tesseractDriver.Recognize(_texture));
        AddToTextDisplay(_tesseractDriver.GetErrorMessage(), true);
        SetImageDisplay();
    }

    private void ClearTextDisplay()
    {
        _text = "";
    }

    private void AddToTextDisplay(string text, bool isError = false)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        _text += (string.IsNullOrWhiteSpace(displayText.text) ? "" : "\n") + text;

        if (isError)
            Debug.LogError(text);
        else
            Debug.Log(text);
    }

    private void LateUpdate()
    {
        displayText.text = _text;
    }

    private void SetImageDisplay()
    {
        RectTransform rectTransform = outputImage.GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            rectTransform.rect.width * _tesseractDriver.GetHighlightedTexture().height / _tesseractDriver.GetHighlightedTexture().width);
        outputImage.texture = _tesseractDriver.GetHighlightedTexture();
    }
}