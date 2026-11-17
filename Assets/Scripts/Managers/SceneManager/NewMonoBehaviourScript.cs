using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("SCENE SETTINGS")]
    [SerializeField] private string sceneToLoad;

    private void Start()
    {
        // CALLING STRAIGHT AWAY FOR DEBUG
        // LoadSceneAdditively();
    }

    public void LoadSceneAdditively()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("No scene name specified ", this);
            return;
        }

        // ALREADY LOADED CHECK
        Scene scene = SceneManager.GetSceneByName(sceneToLoad);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.Log($"Triggering additive load for: {sceneToLoad}");
            SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogWarning($"Scene '{sceneToLoad}' is already loaded");
        }
    }

    // PUBLIC METHOD TO UNLOAD (DELETE) AND RELOAD THE ADDITIVE SCENE
    public void UnloadAndReloadScene()
    {
        StartCoroutine(UnloadAndReloadRoutine());
    }

    private IEnumerator UnloadAndReloadRoutine()
    {
        Scene scene = SceneManager.GetSceneByName(sceneToLoad);

        // UNLOAD IF LOADED
        if (scene.IsValid() && scene.isLoaded)
        {
            Debug.Log($"Unloading additive scene: {sceneToLoad}");
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneToLoad);

            if (unloadOperation != null)
            {
                yield return unloadOperation;
            }
        }

        yield return null;

        // RELOAD FRESH
        Debug.Log($"Reloading additive scene: {sceneToLoad}");
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        yield return loadOperation;
    }
}