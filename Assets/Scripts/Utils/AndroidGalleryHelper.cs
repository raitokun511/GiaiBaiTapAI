using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class AndroidGalleryHelper
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    private static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    private static AndroidJavaObject chooserIntent;
    private static int PICK_IMAGE_REQUEST = 1;

    public static Task<string> PickImageFromGallery()
    {
        var tcs = new TaskCompletionSource<string>();

        using (var intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.PICK"))
        {
            intent.Call<AndroidJavaObject>("setType", "image/*");
            chooserIntent = AndroidIntent.CreateChooser(intent, "Chọn ảnh");
            currentActivity.Call("startActivityForResult", chooserIntent, PICK_IMAGE_REQUEST);

            // Lắng nghe kết quả trả về từ ActivityResult
            AndroidIntent.GetActivityResult(PICK_IMAGE_REQUEST).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    var result = task.Result;
                    if (result.ResultCode == AndroidIntent.Result.Ok)
                    {
                        using (var uri = result.Intent.Call<AndroidJavaObject>("getData"))
                        {
                            if (uri != null)
                            {
                                string imagePath = GetPathFromUri(uri);
                                tcs.SetResult(imagePath);
                            }
                            else
                            {
                                tcs.SetResult(null);
                            }
                        }
                    }
                    else
                    {
                        tcs.SetResult(null); // Người dùng đã hủy hoặc có lỗi
                    }
                }
                else
                {
                    tcs.SetException(task.Exception);
                }
            });
        }

        return tcs.Task;
    }

    private static string GetPathFromUri(AndroidJavaObject uri)
    {
        string filePath = null;
        using (var contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
        {
            using (var cursor = contentResolver.Call<AndroidJavaObject>("query", uri, null, null, null, null))
            {
                if (cursor != null && cursor.Call<bool>("moveToFirst"))
                {
                    int columnIndex = cursor.Call<int>("getColumnIndexOrThrow", "_data");
                    filePath = cursor.Call<string>("getString", columnIndex);
                }
                cursor?.Call("close");
            }
        }
        return filePath;
    }
#else
    public static async Task<string> PickImageFromGallery()
    {
        Debug.LogWarning("Chức năng chọn ảnh từ gallery chỉ hoạt động trên Android device.");
        await Task.Yield();
        return null;
    }
#endif
}
