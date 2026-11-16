using UnityEngine;
using UnityEngine.SceneManagement;


// QUICK AND DIRTY - ADD A SCENE TO THE EXISITNG GAME MANAGER SCENE

public class SceneLoader : MonoBehaviour
{
    [Header("SCENE SETTINGS")]
    [SerializeField] private string sceneToLoad;

    private void Start()
    {
        // CALLING STRAIGHT AWAY FOR DEBUG
        // CALL THIS IN ANOTHER SCRIPT LIKE UI FUNCTION
        LoadSceneAdditively();
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
        if (!scene.isLoaded)
        {
            Debug.Log($"Triggering additive load for: {sceneToLoad}");
            SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogWarning($"Scene '{sceneToLoad}' is already loaded");
        }
    }
}