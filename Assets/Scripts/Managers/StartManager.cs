using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    public GameObject loadingScreen;
    public UnityEngine.UI.Image loadingBarFill;
    public TMP_Text buttonHoverText;

    public void Start()
    {
        buttonHoverText.text = "";
    }

    public void StartDemoScene()
    {
        // Use a coroutine to load the Scene in the background
        StartCoroutine(LoadSceneAsync("Demo"));
    }

    public IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        loadingScreen.SetActive(true);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            float progressValue = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            loadingBarFill.fillAmount = progressValue;
            yield return null;
        }
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    public void OnHoverButtonEnter(string text)
    {
        buttonHoverText.text = text;
    }

    public void OnHoverButtonExit()
    {
        buttonHoverText.text = "";
    }
}
