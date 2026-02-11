using UnityEngine;

public static class StudentIdentity
{
    private const string KEY = "STUDENT_ID_V1";

    public static bool HasId()
    {
        return PlayerPrefs.HasKey(KEY) && !string.IsNullOrEmpty(PlayerPrefs.GetString(KEY));
    }

    public static string GetId()
    {
        return PlayerPrefs.GetString(KEY, "");
    }

    public static void SetId(string id)
    {
        PlayerPrefs.SetString(KEY, id);
        PlayerPrefs.Save();
    }

    public static void ClearId()
    {
        PlayerPrefs.DeleteKey(KEY);
        PlayerPrefs.Save();
    }
}
