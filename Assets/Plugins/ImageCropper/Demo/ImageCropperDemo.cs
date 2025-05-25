using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ImageCropperNamespace
{
	public class ImageCropperDemo : MonoBehaviour
	{
		public RawImage croppedImageHolder;
		public Text croppedImageSize;

		public Toggle ovalSelectionInput, autoZoomInput;
		public InputField minAspectRatioInput, maxAspectRatioInput;

		public RectTransform canvasRectTransform;
        public RawImage displayImage;
        public WebCamTexture webcamTexture;

		public Text output;
        public Canvas targetCanvas;
        public WebCamFullScreenCover webCamFullScreenCover;


        private void Start()
        {
            /*
            webcamTexture = new WebCamTexture();
            displayImage.texture = webcamTexture;
            webcamTexture.Play();

			//AdjustToCanvas();
            //displayImage.rectTransform.sizeDelta = new Vector2(webcamTexture.width, webcamTexture.height);
            if (webcamTexture.videoVerticallyMirrored)
            {
                displayImage.rectTransform.localScale = new Vector3(1, -1, 1);
            }

            displayImage.rectTransform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle);
            */
        }

        private void Update()
        {
            
            if (webcamTexture != null && webcamTexture.isPlaying && webcamTexture.didUpdateThisFrame)
            {
                if (displayImage.texture == null || displayImage.rectTransform.sizeDelta == Vector2.zero) // Chỉ điều chỉnh lần đầu hoặc khi kích thước là zero
                {
                    // Gán texture cho RawImage nếu chưa có
                    if (displayImage.texture == null)
                    {
                        displayImage.texture = webcamTexture;
                        // Có thể cần xoay RawImage nếu camera của thiết bị trả về ảnh xoay
                        // displayImage.transform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle);
                    }
                    AdjustImageSize();
                }
            }
            
        }
        public void AdjustImageSize()
        {
            if (webcamTexture == null || !webcamTexture.isPlaying || displayImage == null || targetCanvas == null)
            {
                Debug.LogWarning("WebCamTexture, DisplayImage hoặc TargetCanvas chưa được gán hoặc webcam chưa chạy.");
                return;
            }

            float imageWidth = webcamTexture.width;
            float imageHeight = webcamTexture.height;

            RectTransform canvasRectTransform = targetCanvas.GetComponent<RectTransform>();
            float canvasWidth = canvasRectTransform.rect.width;
            float canvasHeight = canvasRectTransform.rect.height;

            if (imageWidth <= 0 || imageHeight <= 0 || canvasWidth <= 0 || canvasHeight <= 0)
            {
                Debug.LogWarning("Kích thước ảnh hoặc canvas không hợp lệ.");
                return;
            }

            // 1. Tính tỷ lệ co giãn theo chiều rộng
            float scaleRatioWidth = canvasWidth / imageWidth;

            // 2. Tính tỷ lệ co giãn theo chiều cao
            float scaleRatioHeight = canvasHeight / imageHeight;

            // 3. Chọn tỷ lệ co giãn nhỏ nhất
            float finalScale = Mathf.Min(scaleRatioWidth, scaleRatioHeight);

            // 4. Tính toán kích thước hiển thị cuối cùng cho ảnh
            float displayImgWidth = imageWidth * finalScale;
            float displayImgHeight = imageHeight * finalScale;

            // 5. Áp dụng kích thước mới cho RectTransform của RawImage
            RectTransform imageRectTransform = displayImage.GetComponent<RectTransform>();
            imageRectTransform.sizeDelta = new Vector2(displayImgWidth, displayImgHeight);

            // (Tùy chọn) Căn giữa RawImage trong Canvas (nếu Canvas là cha trực tiếp)
            // imageRectTransform.anchoredPosition = Vector2.zero;

            Debug.Log($"Kích thước WebCam: {imageWidth}x{imageHeight}");
            Debug.Log($"Kích thước Canvas: {canvasWidth}x{canvasHeight}");
            Debug.Log($"Tỷ lệ tính toán: WidthScale={scaleRatioWidth}, HeightScale={scaleRatioHeight}, FinalScale={finalScale}");
            Debug.Log($"Kích thước hiển thị RawImage: {displayImgWidth}x{displayImgHeight}");
        }

        void AdjustToCanvas()
        {
            // Lấy kích thước của Canvas
            float canvasWidth = canvasRectTransform.rect.width;
            float canvasHeight = canvasRectTransform.rect.height;

            // Lấy kích thước của WebCamTexture
            float imageWidth = webcamTexture.width;
            float imageHeight = webcamTexture.height;

			Debug.Log("Canvas:" + canvasWidth + ":" + canvasHeight + "  image:" + imageWidth + ":" + imageHeight);
			output.text = "Canvas:" + canvasWidth + ":" + canvasHeight + "  image:" + imageWidth + ":" + imageHeight;

            bool isWidthMin = imageWidth / canvasWidth < imageHeight / canvasHeight;

			// Áp dụng scale cho displayImage
			//displayImage.rectTransform.sizeDelta = new Vector2(imageWidth * scaleFactor, imageHeight * scaleFactor);
			float ratio = isWidthMin ? imageWidth / canvasWidth : imageHeight / canvasHeight;
            displayImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageWidth * ratio);
            displayImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageHeight * ratio);
        }

        public void Crop(Action<RawImage> completeCrop)
		{
			// If image cropper is already open, do nothing
			if( ImageCropper.Instance.IsOpen )
				return;

			StartCoroutine( TakeScreenshotAndCrop(() =>
            {
                completeCrop?.Invoke(croppedImageHolder);
            }) );
		}

		private IEnumerator TakeScreenshotAndCrop(Action completeCroppedImage)
		{
			yield return new WaitForEndOfFrame();

			bool ovalSelection = ovalSelectionInput.isOn;
			bool autoZoom = autoZoomInput.isOn;

			float minAspectRatio, maxAspectRatio;
			if( !float.TryParse( minAspectRatioInput.text, out minAspectRatio ) )
				minAspectRatio = 0f;
			if( !float.TryParse( maxAspectRatioInput.text, out maxAspectRatio ) )
				maxAspectRatio = 0f;
            webcamTexture = webCamFullScreenCover.webcam;
            Texture2D screenshot = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false);
            screenshot.SetPixels32(webcamTexture.GetPixels32());
            screenshot.Apply();

            //Texture2D screenshot = new Texture2D( Screen.width, Screen.height, TextureFormat.RGB24, false );
            //screenshot.ReadPixels( new Rect( 0, 0, Screen.width, Screen.height ), 0, 0 );
            //screenshot.Apply();

            ImageCropper.Instance.Show( screenshot, ( bool result, Texture originalImage, Texture2D croppedImage ) =>
			{
				// Destroy previously cropped texture (if any) to free memory
				Destroy( croppedImageHolder.texture, 5f );

				// If screenshot was cropped successfully
				if( result )
				{
					// Assign cropped texture to the RawImage
					croppedImageHolder.enabled = true;
					croppedImageHolder.texture = croppedImage;

					Vector2 size = croppedImageHolder.rectTransform.sizeDelta;
					if( croppedImage.height <= croppedImage.width )
						size = new Vector2( 400f, 400f * ( croppedImage.height / (float) croppedImage.width ) );
					else
						size = new Vector2( 400f * ( croppedImage.width / (float) croppedImage.height ), 400f );
					croppedImageHolder.rectTransform.sizeDelta = size;

					croppedImageSize.enabled = true;
					croppedImageSize.text = "Image size: " + croppedImage.width + ", " + croppedImage.height;
                    completeCroppedImage?.Invoke();
				}
				else
				{
					croppedImageHolder.enabled = false;
					croppedImageSize.enabled = false;
				}

				// Destroy the screenshot as we no longer need it in this case
				Destroy( screenshot );
			},
			settings: new ImageCropper.Settings()
			{
				ovalSelection = ovalSelection,
				autoZoomEnabled = autoZoom,
				imageBackground = Color.clear, // transparent background
				selectionMinAspectRatio = minAspectRatio,
				selectionMaxAspectRatio = maxAspectRatio

			},
			croppedImageResizePolicy: ( ref int width, ref int height ) =>
			{
				// uncomment lines below to save cropped image at half resolution
				//width /= 2;
				//height /= 2;
			} );
		}
	}
}