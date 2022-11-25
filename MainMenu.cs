using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayOffline()
    {
        GameConfiguration.IsOnlineGame = false;
        LoadNextScene();
    }

    public void PlayOnline()
    {
        GameConfiguration.IsOnlineGame = true;
        LoadNextScene();
    }

    // https://youtu.be/zc8ac_qUXQY?t=539
    private void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
