using UnityEngine;

public static class LevelProgress
{
    private const string CURRENT_LEVEL_KEY = "CurrentLevelScene";
    private const string HAS_PROGRESS_KEY = "HasLevelProgress";

    public static void SaveCurrentLevel(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        PlayerPrefs.SetString(CURRENT_LEVEL_KEY, sceneName);
        PlayerPrefs.SetInt(HAS_PROGRESS_KEY, 1);
        PlayerPrefs.Save();

        Debug.Log("progresso guardado: " + sceneName);
    }

    public static bool HasProgress()
    {
        return PlayerPrefs.GetInt(HAS_PROGRESS_KEY, 0) == 1
            && !string.IsNullOrWhiteSpace(GetCurrentLevel());
    }

    public static string GetCurrentLevel()
    {
        return PlayerPrefs.GetString(CURRENT_LEVEL_KEY, "");
    }

    public static void ClearProgress()
    {
        PlayerPrefs.DeleteKey(CURRENT_LEVEL_KEY);
        PlayerPrefs.DeleteKey(HAS_PROGRESS_KEY);
        PlayerPrefs.Save();

        Debug.Log("progresso apagado");
    }
}