using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (UserDataManager.isDataCleared)
        {
            PlayerPrefs.DeleteAll();
            UserDataManager.isDataCleared = false;
        }

        UserDataManager.Load();
        SceneManager.LoadScene(1);
    }
}
