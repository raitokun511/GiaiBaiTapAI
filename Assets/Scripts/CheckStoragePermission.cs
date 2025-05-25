using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class CheckStoragePermission : MonoBehaviour
{
    private bool hasReadPermission;
    private bool hasWritePermission;

    public Text outputText;

    void Start()
    {
        CheckPermissions();
    }

    public void CheckPermissions()
    {
#if UNITY_ANDROID
        hasReadPermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        hasWritePermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);

        Debug.Log("Read Permission: " + hasReadPermission);
        Debug.Log("Write Permission: " + hasWritePermission);

        outputText.text = "Read Permission: " + hasReadPermission + "\n";
        outputText.text += "Write Permission: " + hasWritePermission;

        if (!hasReadPermission || !hasWritePermission)
        {
            // Yêu cầu quyền nếu chưa được cấp
            RequestStoragePermissions();
        }
#else
        // Trên các nền tảng khác, giả sử đã có quyền
        hasReadPermission = true;
        hasWritePermission = true;
        Debug.Log("Permissions assumed on non-Android platform.");
#endif
    }

    private void RequestStoragePermissions()
    {
#if UNITY_ANDROID
        Permission.RequestUserPermission(Permission.ExternalStorageRead);
        Permission.RequestUserPermission(Permission.ExternalStorageWrite);
#endif
    }

    // Bạn có thể thêm hàm này để kiểm tra lại quyền sau khi người dùng phản hồi yêu cầu
    void OnGUI()
    {
#if UNITY_ANDROID
        if (!hasReadPermission || !hasWritePermission)
        {
            if (GUI.Button(new Rect(10, 10, 200, 50), "Request Storage Permissions"))
            {
                RequestStoragePermissions();
            }
        }
        else
        {
            GUI.Label(new Rect(10, 70, 200, 30), "Storage Permissions Granted!");
        }
#endif
    }

    // Hàm callback được gọi khi người dùng phản hồi yêu cầu quyền (tùy chọn)
    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            CheckPermissions();
        }
    }
}
