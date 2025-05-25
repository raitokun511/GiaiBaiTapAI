using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class ExpandingInputFieldInLayout : MonoBehaviour
{
    private TMP_InputField inputField;
    private RectTransform rectTransform;
    private ContentSizeFitter contentSizeFitter;
    private VerticalLayoutGroup parentLayoutGroup;

    [Tooltip("Padding thêm vào chiều cao của Input Field (tính bằng đơn vị UI)")]
    public float verticalPadding = 10f;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        rectTransform = GetComponent<RectTransform>();

        // Tìm VerticalLayoutGroup ở parent
        parentLayoutGroup = GetComponentInParent<VerticalLayoutGroup>();
        if (parentLayoutGroup == null)
        {
            Debug.LogError("ExpandingInputFieldInLayout cần một VerticalLayoutGroup ở parent!");
            enabled = false; // Vô hiệu hóa script nếu không tìm thấy Layout Group
            return;
        }

        // Thêm ContentSizeFitter nếu chưa có
        contentSizeFitter = GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
        }

        // Thiết lập ContentSizeFitter để tự động điều chỉnh kích thước theo nội dung
        //contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        //contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize; // Hoặc PreferredSize tùy theo nhu cầu chiều ngang

        // Đăng ký sự kiện khi giá trị của Input Field thay đổi
        inputField.onValueChanged.AddListener(OnValueChanged);

        // Đảm bảo Line Type của Input Field là Single Line khi khởi tạo
        //inputField.lineType = TMP_InputField.LineType.SingleLine;
    }

    void OnValueChanged(string text)
    {
        // Kích hoạt ContentSizeFitter để nó tính toán lại kích thướcPreferredSize
        //contentSizeFitter.enabled = false;
        //contentSizeFitter.enabled = true;

        // Lấy chiều cao Preferred Size sau khi ContentSizeFitter tính toán
        float preferredHeight = 50 + (inputField.text.Length / 30) * 25;
            //LayoutUtility.GetPreferredHeight(inputField.textComponent.rectTransform);

        // Đặt chiều cao Preferred Height cho ContentSizeFitter
        //contentSizeFitter.preferredHeight = preferredHeight + verticalPadding;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight + verticalPadding);


        // Đánh dấu layout group cha là cần được cập nhật lại
        if (parentLayoutGroup != null)
        {
            LayoutRebuilder.MarkLayoutForRebuild(parentLayoutGroup.GetComponent<RectTransform>());
        }

        // Đảm bảo Line Type vẫn là Single Line
        //inputField.lineType = TMP_InputField.LineType.SingleLine;

        // Cập nhật lại hiển thị
        inputField.ForceLabelUpdate();
    }

    public void SetText(string text)
    {
        inputField.text = text;
        OnValueChanged(text);
    }
}