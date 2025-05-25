using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImagePicker : MonoBehaviour
{
    public RawImage targetRawImage; // Kéo thả RawImage UI vào đây trong Inspector
    public Button pickImageButton;

    async void Start()
    {
        if (pickImageButton != null)
        {
            pickImageButton.onClick.AddListener(OnPickImageButtonClicked);
        }
        else
        {
            Debug.LogError("Nút Pick Image chưa được gán!");
        }
    }

    async void OnPickImageButtonClicked()
    {
        string imagePath = await AndroidGalleryHelper.PickImageFromGallery();

        if (!string.IsNullOrEmpty(imagePath))
        {
            Debug.Log("Đường dẫn ảnh đã chọn: " + imagePath);
            // Load ảnh từ đường dẫn và hiển thị lên RawImage
            StartCoroutine(LoadImageToRawImage(imagePath));
        }
        else
        {
            Debug.Log("Không có ảnh nào được chọn hoặc có lỗi xảy ra.");
        }
    }

    IEnumerator LoadImageToRawImage(string path)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            if (targetRawImage != null)
            {
                targetRawImage.texture = texture;
            }
        }
        else
        {
            Debug.LogError("Không thể load ảnh từ đường dẫn: " + www.error);
        }
    }
}
