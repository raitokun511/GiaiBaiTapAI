using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCamFullScreenCover : MonoBehaviour
{
    [Tooltip("RawImage để hiển thị WebCamTexture.")]
    public RawImage displayImage;

    [Tooltip("Tên camera mong muốn (để trống để dùng camera mặc định).")]
    public string preferredCameraName = "";

    [Tooltip("Lật ngang hình ảnh nếu là camera trước (hiệu ứng gương).")]
    public bool mirrorFrontCamera = true;

    private WebCamTexture webcamTexture;
    private bool webcamInitialized = false;
    private Vector2 lastScreenSize;
    private int lastRotationAngle = 0;
    private bool lastIsFrontFacing = false;
    private bool lastIsVerticallyMirrored = false;

    public WebCamTexture webcam
    {
        get { return webcamTexture; }
    }

    public void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError("DisplayImage chưa được gán!");
            this.enabled = false;
            return;
        }

        // Cấu hình RawImage để chiếm toàn bộ Canvas
        RectTransform rawImageRect = displayImage.GetComponent<RectTransform>();
        rawImageRect.anchorMin = new Vector2(0, 0);
        rawImageRect.anchorMax = new Vector2(1, 1);
        rawImageRect.offsetMin = Vector2.zero;
        rawImageRect.offsetMax = Vector2.zero;
        rawImageRect.localScale = Vector3.one; // Đảm bảo scale ban đầu là 1

        StartCoroutine(InitializeWebcam());
    }

    IEnumerator InitializeWebcam()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("Không tìm thấy camera nào!");
            yield break;
        }

        WebCamDevice deviceToUse;
        if (!string.IsNullOrEmpty(preferredCameraName))
        {
            bool found = false;
            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                if (WebCamTexture.devices[i].name == preferredCameraName)
                {
                    deviceToUse = WebCamTexture.devices[i];
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Debug.LogWarning($"Không tìm thấy camera có tên '{preferredCameraName}'. Sử dụng camera mặc định.");
                deviceToUse = WebCamTexture.devices[0];
            }
            else
            {
                deviceToUse = WebCamTexture.devices[0]; // Fallback an toàn
                for (int i = 0; i < WebCamTexture.devices.Length; i++) // Tìm lại deviceToUse
                {
                    if (WebCamTexture.devices[i].name == preferredCameraName)
                    {
                        deviceToUse = WebCamTexture.devices[i];
                        break;
                    }
                }
            }
        }
        else
        {
            deviceToUse = WebCamTexture.devices[0];
        }

        webcamTexture = new WebCamTexture(deviceToUse.name);
        displayImage.texture = webcamTexture;
        webcamTexture.Play();

        // Chờ webcam thực sự khởi tạo và có kích thước
        yield return new WaitUntil(() => webcamTexture.width > 100 && webcamTexture.height > 100);

        webcamInitialized = true;
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        lastRotationAngle = webcamTexture.videoRotationAngle;
        lastIsFrontFacing = deviceToUse.isFrontFacing;
        lastIsVerticallyMirrored = webcamTexture.videoVerticallyMirrored;

        SetupDisplay();
    }
    public void CloseWebcam()
    {
        if (webcamTexture != null)
        {
            if (webcamTexture.isPlaying)
            {
                webcamTexture.Stop();
            }
            Destroy(webcamTexture);
            webcamTexture = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!webcamInitialized || webcamTexture == null || !webcamTexture.isPlaying)
        {
            return;
        }

        // Kiểm tra nếu có sự thay đổi cần cập nhật lại hiển thị
        bool needsUpdate = false;
        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            needsUpdate = true;
        }
        if (webcamTexture.videoRotationAngle != lastRotationAngle)
        {
            lastRotationAngle = webcamTexture.videoRotationAngle;
            needsUpdate = true;
        }
        // Kiểm tra isFrontFacing có thể không thay đổi runtime, nhưng videoVerticallyMirrored có thể
        if (webcamTexture.videoVerticallyMirrored != lastIsVerticallyMirrored)
        {
            lastIsVerticallyMirrored = webcamTexture.videoVerticallyMirrored;
            needsUpdate = true;
        }
        // Thêm kiểm tra isFrontFacing nếu camera có thể thay đổi runtime
        // WebCamDevice currentDevice; // Cần logic để lấy device hiện tại nếu có thể thay đổi
        // if (currentDevice.isFrontFacing != lastIsFrontFacing) { ... }


        if (needsUpdate || webcamTexture.didUpdateThisFrame) // Luôn cập nhật nếu texture có frame mới để đảm bảo uvRect đúng
        {
            SetupDisplay();
        }
    }


    void SetupDisplay()
    {
        if (webcamTexture == null || !webcamTexture.isPlaying || webcamTexture.width <= 100)
        {
            return; // Chưa sẵn sàng
        }

        // 1. Xoay RawImage để bù cho videoRotationAngle
        displayImage.rectTransform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle);

        // 2. Xử lý hiệu ứng gương cho camera trước
        float scaleX = 1f;
        bool isFrontFacing = false;
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].name == webcamTexture.deviceName)
            {
                isFrontFacing = devices[i].isFrontFacing;
                break;
            }
        }
        lastIsFrontFacing = isFrontFacing; // Cập nhật trạng thái

        if (mirrorFrontCamera && isFrontFacing)
        {
            scaleX = -1f;
        }
        // Áp dụng scale. Lưu ý: nếu videoRotationAngle là 90 hoặc 270,
        // việc scale X có thể ảnh hưởng đến chiều Y sau khi xoay.
        // Để đơn giản, chúng ta scale trước khi tính uvRect.
        // Tuy nhiên, scale của RectTransform nên được áp dụng cẩn thận.
        // Một cách tiếp cận khác là lật uvRect.x thay vì scale transform.
        // Hiện tại, giữ nguyên scale transform:
        displayImage.rectTransform.localScale = new Vector3(scaleX, 1f, 1f);


        // 3. Tính toán uvRect để "cover" (Aspect Fill)
        float textureWidth = webcamTexture.width;
        float textureHeight = webcamTexture.height;

        // Kích thước của container (RawImage) đã được set để full màn hình
        // và đã được xoay/scale. rect.width/height là kích thước local của nó.
        float containerWidth = displayImage.rectTransform.rect.width;
        float containerHeight = displayImage.rectTransform.rect.height;

        if (containerWidth <= 0 || containerHeight <= 0 || textureWidth <= 0 || textureHeight <= 0) return;

        float textureAspect = textureWidth / textureHeight;
        float containerAspect = containerWidth / containerHeight;

        float uv_x = 0, uv_y = 0, uv_w = 1, uv_h = 1;

        if (textureAspect > containerAspect)
        {
            // Texture rộng hơn so với container (ví dụ: texture 16:9, container 4:3 màn hình dọc)
            // Cần crop hai bên của texture. Chiều cao của uvRect là 1.
            uv_h = 1;
            uv_w = containerAspect / textureAspect; // Tỷ lệ của chiều rộng mới so với chiều rộng gốc
            uv_x = (1 - uv_w) / 2f; // Căn giữa theo chiều ngang
            uv_y = 0;
        }
        else
        {
            // Texture cao hơn so với container (ví dụ: texture 4:3, container 16:9 màn hình ngang)
            // Cần crop trên dưới của texture. Chiều rộng của uvRect là 1.
            uv_w = 1;
            uv_h = textureAspect / containerAspect; // Tỷ lệ của chiều cao mới so với chiều cao gốc
            uv_x = 0;
            uv_y = (1 - uv_h) / 2f; // Căn giữa theo chiều dọc
        }

        Rect finalUvRect = new Rect(uv_x, uv_y, uv_w, uv_h);

        // 4. Xử lý videoVerticallyMirrored (dữ liệu texture gốc bị lật)
        if (webcamTexture.videoVerticallyMirrored)
        {
            // Lật lại uvRect theo chiều dọc
            // y mới = 1 - y_cũ - height_cũ
            finalUvRect.y = 1f - finalUvRect.y - finalUvRect.height;
        }

        displayImage.uvRect = finalUvRect;
    }

    void OnDestroy()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}
