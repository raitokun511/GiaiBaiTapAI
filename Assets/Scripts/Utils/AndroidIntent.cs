#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

public static class AndroidIntent
{
    private static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    private static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

    public static AndroidJavaObject CreateChooser(AndroidJavaObject target, string title)
    {
        using (var intentClass = new AndroidJavaClass("android.content.Intent"))
        using (var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", target, title))
        {
            return chooser;
        }
    }

    // Thêm các hàm khác cho xử lý ActivityResult nếu cần
    public static System.Threading.Tasks.Task<ActivityResult> GetActivityResult(int requestCode)
    {
        var tcs = new System.Threading.Tasks.TaskCompletionSource<ActivityResult>();
        using (var activityResultCallback = new AndroidActivityResultCallback(requestCode, tcs))
        using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var unityPlayerActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var unityPlayerContext = unityPlayerActivity.Call<AndroidJavaObject>("getApplicationContext"))
        using (var localBroadcastManagerClass = new AndroidJavaClass("androidx.localbroadcastmanager.content.LocalBroadcastManager"))
        using (var localBroadcastManager = localBroadcastManagerClass.CallStatic<AndroidJavaObject>("getInstance", unityPlayerContext))
        using (var intentFilter = new AndroidJavaObject("android.content.IntentFilter", "UnityActivityResult-" + requestCode))
        {
            localBroadcastManager.Call("registerReceiver", activityResultCallback, intentFilter);
        }
        return tcs.Task;
    }

    private class AndroidActivityResultCallback : AndroidJavaProxy
    {
        private int requestCode;
        private System.Threading.Tasks.TaskCompletionSource<ActivityResult> taskCompletionSource;

        public AndroidActivityResultCallback(int requestCode, System.Threading.Tasks.TaskCompletionSource<ActivityResult> taskCompletionSource) : base("android.content.BroadcastReceiver")
        {
            this.requestCode = requestCode;
            this.taskCompletionSource = taskCompletionSource;
        }

        public void onReceive(AndroidJavaObject context, AndroidJavaObject intent)
        {
            int receivedRequestCode = intent.Call<int>("getIntExtra", "requestCode", -1);
            if (receivedRequestCode == requestCode)
            {
                int resultCode = intent.Call<int>("getIntExtra", "resultCode", 0);
                AndroidJavaObject data = intent.Call<AndroidJavaObject>("getParcelableExtra", "intent");
                taskCompletionSource.SetResult(new ActivityResult(resultCode, data));

                using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var unityPlayerActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var unityPlayerContext = unityPlayerActivity.Call<AndroidJavaObject>("getApplicationContext"))
                using (var localBroadcastManagerClass = new AndroidJavaClass("androidx.localbroadcastmanager.content.LocalBroadcastManager"))
                using (var localBroadcastManager = localBroadcastManagerClass.CallStatic<AndroidJavaObject>("getInstance", unityPlayerContext))
                {
                    localBroadcastManager.Call("unregisterReceiver", this);
                }
            }
        }
    }

    public struct ActivityResult
    {
        public int ResultCode { get; }
        public AndroidJavaObject Intent { get; }

        public ActivityResult(int resultCode, AndroidJavaObject intent)
        {
            ResultCode = resultCode;
            Intent = intent;
        }

        public static class Result
        {
            public static int Ok = -1;
            public static int Canceled = 0;
        }
    }
}
#else
using System.Threading.Tasks;
using UnityEngine;

public static class AndroidIntent
{
    public static AndroidJavaObject CreateChooser(AndroidJavaObject target, string title)
    {
        Debug.LogWarning("AndroidIntent only works on Android.");
        return null;
    }

    public static Task<ActivityResult> GetActivityResult(int requestCode)
    {
        Debug.LogWarning("AndroidIntent only works on Android.");
        var tcs = new TaskCompletionSource<ActivityResult>();
        tcs.SetResult(default(ActivityResult));
        return tcs.Task;
    }

    public struct ActivityResult
    {
        public int ResultCode { get; }
        public AndroidJavaObject Intent { get; }

        public ActivityResult(int resultCode, AndroidJavaObject intent)
        {
            ResultCode = resultCode;
            Intent = intent;
        }

        public static class Result
        {
            public static int Ok = -1;
            public static int Canceled = 0;
        }
    }
}
#endif