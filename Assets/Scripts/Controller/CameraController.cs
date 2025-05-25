using System.Collections;
using System.Collections.Generic;
using ImageCropperNamespace;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    SolveController controller;
    [SerializeField]
    ImageCropperDemo cropImage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Back()
    {
        CloseCameraPanel();
    }

    void CloseCameraPanel()
    {
        this.gameObject.SetActive(false);
        controller.CloseCamera();
    }

    public void SelectedImageFromGallery()
    {

    }
    public void CaptureImage()
    {
        cropImage.Crop(CroppedComplete);
    }

    void CroppedComplete(RawImage resultImage)
    {
        Debug.Log("Cropped Complete");

        controller.SendImageProblem(resultImage);
        CloseCameraPanel();
    }
}
