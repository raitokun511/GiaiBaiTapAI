using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotiPopup : MonoBehaviour
{
    public static NotiPopup instance;
    [SerializeField]
    RectTransform titlePanel;
    [SerializeField]
    Text titleText;
    [SerializeField]
    Text contentText;
    [SerializeField]
    Button okButton;
    [SerializeField]
    Text okButtonText;
    [SerializeField]
    Button cancelButton;
    [SerializeField]
    Text cancelButtonText;

    private System.Action onOkAction;
    private System.Action onCancelAction;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Call(string title, string message,
                              string okBtnTxt, System.Action onOk,
                              string cancelBtnTxt = "", System.Action onCancel = null)
    {

        titleText.text = title;
        contentText.text = message;
        okButtonText.text = okBtnTxt;

        onOkAction = onOk;
        onCancelAction = onCancel;

        if (cancelBtnTxt != "")
        {
            cancelButton.gameObject.SetActive(true);
            cancelButtonText.text = cancelBtnTxt;
        }
        else
        {
            cancelButton.gameObject.SetActive(false);
        }

        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(OnOkButtonClicked);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    private void OnOkButtonClicked()
    {
        onOkAction?.Invoke();
        ClosePopup();
    }

    private void OnCancelButtonClicked()
    {
        onCancelAction?.Invoke();
        ClosePopup();
    }

    private void ClosePopup()
    {
        this.gameObject.SetActive(false);
    }

}
