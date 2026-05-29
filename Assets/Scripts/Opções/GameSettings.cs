using UnityEngine;

public static class GameSettings
{
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";

    private const float DEFAULT_MASTER_VOLUME = 1f;
    private const float DEFAULT_MOUSE_SENSITIVITY = 0.12f;

    public static float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, DEFAULT_MASTER_VOLUME);
    }

    public static void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
        PlayerPrefs.Save();

        AudioListener.volume = value;
    }

    public static float GetMouseSensitivity()
    {
        return PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, DEFAULT_MOUSE_SENSITIVITY);
    }

    public static void SetMouseSensitivity(float value)
    {
        value = Mathf.Clamp(value, 0.02f, 0.5f);

        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, value);
        PlayerPrefs.Save();

        FirstPersonPlayer player = Object.FindFirstObjectByType<FirstPersonPlayer>();

        if (player != null)
        {
            player.SetMouseSensitivity(value);
        }
    }

    public static void LoadSettings()
    {
        AudioListener.volume = GetMasterVolume();
    }
}