using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadingSceneController
{
    public static string NextSceneName;

    public static void LoadScene(string sceneName)
    {
        NextSceneName = sceneName;
        SceneManager.LoadScene("Loading");
    }
}