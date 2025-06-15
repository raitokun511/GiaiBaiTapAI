using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;


public class CallGallery : MonoBehaviour
{
    public static CallGallery Instance;
    [SerializeField]
    RawImage displayImage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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

    public void OpenGallery()
    {
        NativeGallery.GetImageFromGallery((path) => {
            if (!string.IsNullOrEmpty(path))
            {
                Debug.Log("Image selected! Path: " + path);
                // Bây giờ chúng ta có đường dẫn, hãy tải ảnh vào Texture2D
                LoadImageFromPath(path);
            }
            else
            {
                // Nếu path là null hoặc rỗng, có nghĩa là người dùng đã hủy hoặc có lỗi.
                Debug.LogWarning("No image selected or an error occurred. Path was empty/null.");
            }
        },
            "Chọn ảnh của bạn", // Tiêu đề tùy chọn
            "image/*"           // Loại MIME tùy chọn
        );
    }

    // Hàm để tải ảnh từ đường dẫn vào Texture2D và hiển thị
    private void LoadImageFromPath(string path)
    {
        if (File.Exists(path))
        {
            // Đọc tất cả byte từ file ảnh
            byte[] fileData = File.ReadAllBytes(path);

            // Tạo một Texture2D mới. Kích thước ban đầu không quan trọng
            // vì LoadImage sẽ điều chỉnh nó theo kích thước của ảnh.
            Texture2D texture = new Texture2D(2, 2);

            // Tải dữ liệu ảnh vào Texture2D
            if (texture.LoadImage(fileData)) // LoadImage sẽ tự động xử lý các định dạng ảnh phổ biến (PNG, JPG)
            {
                Debug.Log($"Image loaded into Texture2D: {texture.width}x{texture.height}");

                // Gán Texture2D này cho RawImage để hiển thị trong UI
                if (displayImage != null)
                {
                    displayImage.texture = texture;
                    // Điều chỉnh kích thước RawImage nếu cần
                    displayImage.SetNativeSize();
                    SolveController.instance.SendImageProblem(displayImage);
                    
                }
                else
                {
                    Debug.LogWarning("RawImage display reference is not set in inspector.");
                }
            }
            else
            {
                Debug.LogError("Failed to load image data into Texture2D. The file might be corrupted or not a valid image.");
            }
        }
        else
        {
            Debug.LogError("Image file not found at path: " + path);
        }
    }
}
