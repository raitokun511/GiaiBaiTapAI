using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class GAToggleGroup : MonoBehaviour
{
    [SerializeField]
    ToggleGroup ToggleGroup;
    

    // Start is called before the first frame update
    void Start()
    {
        if (ToggleGroup == null)
        {
            Debug.LogError("toggle group null");
            return;
        }
        foreach(Toggle toggle in ToggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle, isOn));
        }

        UpdateSelectedDifficultyInfo();
    }
    private void OnToggleValueChanged(Toggle changedToggle, bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Toggle '" + changedToggle.transform.parent.name + "' vừa được BẬT.");
            UpdateSelectedDifficultyInfo();
        }
        else
        {
            // Nếu Toggle này vừa bị tắt (do một Toggle khác được chọn hoặc Allow Switch Off)
            Debug.Log("Toggle '" + changedToggle.transform.parent.name + "' vừa được TẮT.");
            // Không cần cập nhật ở đây nếu bạn luôn muốn hiển thị Toggle đang ON duy nhất
            // (vì UpdateSelectedDifficultyInfo() sẽ làm điều đó)
        }
    }

    // Phương thức chính để lấy Toggle đang được chọn
    public Toggle GetCurrentlySelectedDifficulty()
    {
        // 'ActiveToggles()' trả về một IEnumerable<Toggle> của tất cả các Toggle hiện đang ON.
        // Vì Toggle Group mặc định chỉ cho phép một Toggle ON cùng lúc,
        // chúng ta có thể sử dụng .FirstOrDefault() để lấy Toggle đó.
        // Nếu 'Allow Switch Off' được bật và không có Toggle nào ON, nó sẽ trả về null.
        return ToggleGroup.ActiveToggles().FirstOrDefault();
    }

    // Phương thức để cập nhật Text UI (nếu có)
    void UpdateSelectedDifficultyInfo()
    {
        Toggle selectedToggle = GetCurrentlySelectedDifficulty();
        if (selectedToggle != null)
        {
            string difficultyName = selectedToggle.name.Replace("Toggle", ""); // Ví dụ: "Easy" từ "EasyToggle"
            Debug.Log("Mức độ khó hiện tại được chọn: " + difficultyName);
           
        }
        else
        {
            Debug.Log("Hiện không có mức độ khó nào được chọn.");
            
        }
    }

    // Ví dụ về cách bạn có thể thiết lập một Toggle làm mặc định từ code
    public void SetDifficulty(string difficultyName)
    {
        foreach (Toggle toggle in ToggleGroup.GetComponentsInChildren<Toggle>())
        {
            if (toggle.name.Contains(difficultyName)) // Ví dụ: "EasyToggle" chứa "Easy"
            {
                toggle.isOn = true; // Đặt isOn = true sẽ tự động tắt các Toggle khác trong nhóm
                UpdateSelectedDifficultyInfo();
                return;
            }
        }
        Debug.LogWarning("Không tìm thấy Toggle với tên chứa: " + difficultyName);
    }

    // Quan trọng: Gỡ bỏ listener khi GameObject bị hủy để tránh rò rỉ bộ nhớ
    void OnDestroy()
    {
        if (ToggleGroup != null)
        {
            foreach (Toggle toggle in ToggleGroup.GetComponentsInChildren<Toggle>())
            {
                // Chỉ gỡ bỏ listener nếu nó không null
                if (toggle != null)
                {
                    toggle.onValueChanged.RemoveAllListeners(); // Hoặc RemoveListener cụ thể nếu bạn quản lý nhiều listener
                }
            }
        }
    }
}
