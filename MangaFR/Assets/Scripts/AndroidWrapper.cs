using System;
using UnityEngine;

public class AndroidWrapper : MonoBehaviour
{
    private AndroidJavaObject javaClass;

    private string javaPackageName = "com.mangafr.androidplugin";
    private string javaClassName = "AndroidPlugin";

    string hexColor = "#0F0F0F";

    void Start()
    {
        /*
#if UNITY_ANDROID
        //Get the java class we are working with
        javaClass = new AndroidJavaObject($"{javaPackageName}.{javaClassName}");

        //https://zehfernando.com/2015/unity-tidbits-changing-the-visibility-of-androids-navigation-and-status-bars-and-implementing-immersive-mode/
        //Send the activity and window to java -> If this fail, it's impossible to anything
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (var window = activity.Call<AndroidJavaObject>("getWindow"))
                {
                    javaClass.Call("GetActivity", activity, window);
                }
            }
        }

        //Dislay navigation bar -> when turning fullscreen off, the navigation bar show up (android)
        Screen.fullScreen = false;

        //Display status bar
        //javaClass.Call("StatusBarState");

        javaClass.Call("SetNavigationBarColor", hexColor);
#endif
        */
    }

}
