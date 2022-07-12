using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTool.Tools
{
    public static class Vibration
    {
        public static AndroidJavaClass unityPlayer;
        public static AndroidJavaObject currentActivity;
        public static AndroidJavaObject vibrator;

        /// <summary>
        /// 震动
        /// </summary>
        public static void Vibrate()
        {
            if (isAndroid())
                vibrator.Call("vibrate");
            else
                Handheld.Vibrate();
        }

        /// <summary>
        /// 震动
        /// </summary>
        public static void Vibrate(long milliseconds)
        {
            if (isAndroid())
                vibrator.Call("vibrate", milliseconds);
            else
                Handheld.Vibrate();
        }

        /// <summary>
        /// 震动
        /// </summary>
        public static void Vibrate(long[] pattern, int repeat)
        {
            if (isAndroid())
                vibrator.Call("vibrate", pattern, repeat);
            else
                Handheld.Vibrate();
        }

        /// <summary>
        /// 是否可以震动
        /// </summary>
        /// <returns></returns>
        public static bool HasVibrator()
        {
            return isAndroid();
        }

        /// <summary>
        /// 取消震动
        /// </summary>
        public static void Cancel()
        {
            if (isAndroid())
                vibrator.Call("cancel");
        }

        private static bool isAndroid()
        {
            GetPlatform();
            return (Application.platform == RuntimePlatform.Android);
        }


        private static void GetPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:

                    break;
                case RuntimePlatform.Android:
                    if (unityPlayer == null)
                        unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    if (currentActivity == null)
                        currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    if (vibrator == null)
                        vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    break;
                case RuntimePlatform.IPhonePlayer:

                    break;
            }
        }
    }
}
