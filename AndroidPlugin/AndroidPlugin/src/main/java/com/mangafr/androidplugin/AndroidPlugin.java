package com.mangafr.androidplugin;

import com.unity3d.player.UnityPlayerActivity;

import android.content.Context;
import android.graphics.Color;
import android.os.Build;
import android.os.Bundle;
import android.view.Window;
import android.widget.Toast;

import androidx.annotation.RequiresApi;

@RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
public class AndroidPlugin extends UnityPlayerActivity
{
    private Context mContext = null;
    private Window mWindow = null;

    @Override
    public void onCreate(Bundle saveInstanceState)
    {
        super.onCreate(saveInstanceState);

        mUnityPlayer.setSystemUiVisibility(true);
    }

    public void GetActivity(Context context, Window window)
    {
        mContext = context;
        mWindow = window;
        //CallToast("Activity Successfully Loaded !", Toast.LENGTH_SHORT);
    }

    public void CallToast(final String text, int duration)
    {
		runOnUiThread(() -> Toast.makeText(mContext, text, duration).show());
	}

    public void StatusBarState()
    {
        CallToast("Status fullscreen done!", Toast.LENGTH_SHORT);
        mUnityPlayer.setSystemUiVisibility(true);
        //mWindow.setStatusBarColor(Color.parseColor(rgba));
        CallToast("Status color done!", Toast.LENGTH_SHORT);
    }
    //hide 1024
    //show 2048

    public void SetNavigationBarColor(String rgba)
    {
        //CallToast(rgba, Toast.LENGTH_SHORT);
        mWindow.setNavigationBarColor(Color.parseColor(rgba));
        //CallToast("Coloring done", Toast.LENGTH_LONG);
    }
}