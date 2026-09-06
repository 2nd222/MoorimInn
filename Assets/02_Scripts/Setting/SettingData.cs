using System;
using UnityEngine;

public class SettingData
{
    public static Action<float> OnFontSizeChanged;

    private const float MIN_FONT_SIZE = 30f;
    private const float MAX_FONT_SIZE = 50f;

    public static void SetFontSize(float size)
    {
        float clampedSize = Mathf.Clamp(size, MIN_FONT_SIZE, MAX_FONT_SIZE);

        PlayerPrefs.SetFloat("StoryFontSize", clampedSize);
        PlayerPrefs.Save();

        OnFontSizeChanged?.Invoke(clampedSize);
    }

    public static float GetFontSize()
    {
        float savedSize = PlayerPrefs.GetFloat("StoryFontSize", 40f); // ±âº»°ª 40
        return Mathf.Clamp(savedSize, MIN_FONT_SIZE, MAX_FONT_SIZE);
    }
}
