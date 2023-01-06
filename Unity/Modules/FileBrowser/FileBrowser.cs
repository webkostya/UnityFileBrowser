using System.IO;
using UnityEngine;

namespace Modules.FileBrowser
{
    public static class FileBrowser
    {
        private const string Authority = "com.app.mobile.fileprovider";

        public static void OpenFile(string path)
        {
#if UNITY_ANDROID
            OpenAndroidFile(path);
#endif
        }

        /// <summary>
        /// Open Android File
        /// </summary>
        /// <param name="path"></param>
        private static void OpenAndroidFile(string path)
        {
            var mimeType = FileMimeType.GetType(Path.GetExtension(path));

            var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer") as AndroidJavaObject;
            var activity = player.GetStatic<AndroidJavaObject>("currentActivity");

            var intent = new AndroidJavaObject("android.content.Intent");

            intent.Call<AndroidJavaObject>("addFlags", intent.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"));
            intent.Call<AndroidJavaObject>("setAction", intent.GetStatic<string>("ACTION_VIEW"));

            var filePath = GetAndroidFilePath(path, activity);

            intent.Call<AndroidJavaObject>("setType", mimeType);
            intent.Call<AndroidJavaObject>("setData", filePath);

            activity.Call("startActivity", intent);
        }

        /// <summary>
        /// Get Android File Path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        private static AndroidJavaObject GetAndroidFilePath(string path, AndroidJavaObject activity)
        {
            AndroidJavaObject output;

            var apiVersion = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");

            if (apiVersion > 23)
            {
                var provider = new AndroidJavaClass("androidx.core.content.FileProvider");
                var file = new AndroidJavaObject("java.io.File", path);

                var context = activity.Call<AndroidJavaObject>("getApplicationContext");

                output = provider.CallStatic<AndroidJavaObject>("getUriForFile", context, Authority, file);
            }
            else
            {
                var uri = new AndroidJavaClass("android.net.Uri");
                var file = new AndroidJavaObject("java.io.File", path);

                output = uri.CallStatic<AndroidJavaObject>("fromFile", file);
            }

            return output;
        }
    }
}