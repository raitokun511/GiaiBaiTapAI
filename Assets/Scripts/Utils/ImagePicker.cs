using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
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

    public async void OnPickImageButtonClicked()
    {
#if UNITY_ANDROID
        // Kiểm tra và yêu cầu quyền READ_EXTERNAL_STORAGE
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            PermissionCallbacks callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += OnPermissionGranted;
            callbacks.PermissionDenied += OnPermissionDenied;
            callbacks.PermissionDeniedAndDontAskAgain += OnPermissionDeniedAndDontAskAgain;

            Permission.RequestUserPermission(Permission.ExternalStorageRead, callbacks);
            return; // Đợi callback xử lý quyền
        }
#endif
        // Nếu đã có quyền hoặc không phải Android, thì tiến hành chọn ảnh
        await PickImage();
    }
    async Task PickImage()
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

#if UNITY_ANDROID
    private async void OnPermissionGranted(string permissionName)
    {
        Debug.Log($"Permission {permissionName} GRANTED!");
        await PickImage(); // Tiến hành chọn ảnh sau khi được cấp quyền
    }

    private void OnPermissionDenied(string permissionName)
    {
        Debug.Log($"Permission {permissionName} DENIED!");
        // Thông báo cho người dùng rằng không thể chọn ảnh nếu không có quyền
    }

    private void OnPermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"Permission {permissionName} DENIED and DON'T ASK AGAIN!");
        // Hướng dẫn người dùng vào cài đặt để cấp quyền thủ công
    }
#endif
}
