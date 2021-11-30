using Firebase.Storage;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UserDataManager
{
    private const string PROGRESS_KEY = "Progress";

    public static UserProgressData progress;
    public static bool isDataCleared = false;

    public static void LoadFromLocal()
    {
        //Cek apakah ada data yang tersimpan sebagai PROGRESS_KEY
        if (!PlayerPrefs.HasKey(PROGRESS_KEY))
        {
            //Jika tidak ada, maka buat data baru dan upload ke cloud
            progress = new UserProgressData();
            Save(true);
        }
        else
        {
            //Jika ada, maka timpa progress dengan yang sebelumnya
            string json = PlayerPrefs.GetString(PROGRESS_KEY);
            progress = JsonUtility.FromJson<UserProgressData>(json);
        }
    }

    public static IEnumerator LoadFromCloud(System.Action onComplete)
    {
        StorageReference targetStorage = GetTargetCloudStorage();

        bool isComplete = false;
        bool isSuccessfull = false;
        const long MAX_ALLOWED_SIZE = 1024 * 1024; // sama dengan 1 MB
        targetStorage.GetBytesAsync(MAX_ALLOWED_SIZE).ContinueWith((Task<byte[]> task) =>
        {
            if (!task.IsFaulted)
            {
                string json = Encoding.Default.GetString(task.Result);
                progress = JsonUtility.FromJson<UserProgressData>(json);
                isSuccessfull = true;
            }

            isComplete = true;
        });

        while (!isComplete)
        {
            yield return null;
        }

        //Jika sukses mendownload, maka simpatn data hasil download
        if (isSuccessfull)
        {
            Save(true);
        }
        else
        {
            //Jika tidak ada data di cloud, maka load data dari local
            LoadFromLocal();
        }

        onComplete?.Invoke();
    }

    private static StorageReference GetTargetCloudStorage()
    {
        //Gunakan device ID sebagai nama file yang akan disimpan di dalam cloud
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;

        return storage.GetReferenceFromUrl($"{storage.RootReference}/{deviceID}");
    }

    public static void Save(bool uploadToCloud = false)
    {
        string json = JsonUtility.ToJson(progress);
        PlayerPrefs.SetString(PROGRESS_KEY, json);

        if (uploadToCloud)
        {
            AnalyticsManager.SetUserProperties("gold", progress.gold.ToString());

            byte[] data = Encoding.Default.GetBytes(json);
            StorageReference targetStorage = GetTargetCloudStorage();

            targetStorage.PutBytesAsync(data);
        }
    }

    public static bool HasResources(int index)
    {
        return index + 1 <= progress.ResourceLevels.Count;
    }
}