using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoService<SceneController>
{
    private void Awake()
    {
        RegisterService();
    }

    private void OnDestroy()
    {
        UnregisterService();
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Cannot load scene. Scene name is empty.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByIndex(int sceneBuildIndex)
    {
        if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"Cannot load scene. Build index {sceneBuildIndex} is outside build settings.");
            return;
        }

        SceneManager.LoadScene(sceneBuildIndex);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextScene()
    {
        var nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("Cannot load next scene. Current scene is the last scene in build settings.");
            return;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("QuitGame called. Application quit is ignored in the editor.");
#else
        Application.Quit();
#endif
    }
}
