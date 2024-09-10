using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    public void StartDemoScene()
    {
        // Use a coroutine to load the Scene in the background
        StartCoroutine(LoadSceneAsync("Demo"));
    }

    public IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            Debug.Log("Scene loading. Here is where loadings creen logic should go.");
            yield return null;
        }
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
}
