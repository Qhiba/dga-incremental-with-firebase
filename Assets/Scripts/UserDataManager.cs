using UnityEngine;
using UnityEngine.SceneManagement;

public static class UserDataManager
{
    private const string PROGRESS_KEY = "Progress";

    public static UserProgressData progress;
    public static bool isDataCleared = false;

    public static void Load()
    {
        //Cek apakah ada data yang tersimpan sebagai PROGRESS_KEY
        if (!PlayerPrefs.HasKey(PROGRESS_KEY))
        {
            //Jika tidak ada, maka buat data baru
            progress = new UserProgressData();
            Save();
        }
        else
        {
            //Jika ada, maka timpa progress dengan yang sebelumnya
            string json = PlayerPrefs.GetString(PROGRESS_KEY);
            progress = JsonUtility.FromJson<UserProgressData>(json);
        }
    }

    public static void Save()
    {
        string json = JsonUtility.ToJson(progress);
        PlayerPrefs.SetString(PROGRESS_KEY, json);
    }

    public static bool HasResources(int index)
    {
        return index + 1 <= progress.ResourceLevels.Count;
    }

    public static void ClearData()
    {
        isDataCleared = true;
        SceneManager.LoadScene(0);
    }
}