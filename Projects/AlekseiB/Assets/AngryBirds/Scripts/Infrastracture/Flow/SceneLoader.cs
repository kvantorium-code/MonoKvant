using UnityEngine;
using UnityEngine.SceneManagement;

namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>Loads scenes from the build settings, replacing the current scene.</summary>
    public sealed class SceneLoader : ISceneLoader
    {
        public bool CanLoad(string sceneName)
        {
            return Application.CanStreamedLevelBeLoaded(sceneName);
        }

        public bool CanLoad(int buildIndex)
        {
            return buildIndex >= 0 && buildIndex < SceneManager.sceneCountInBuildSettings;
        }

        public void Load(string sceneName)
        {
            if (!CanLoad(sceneName))
            {
                Debug.LogError($"[SceneLoader] Scene '{sceneName}' is not in the build settings.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        public void Load(int buildIndex)
        {
            if (!CanLoad(buildIndex))
            {
                Debug.LogError($"[SceneLoader] Build index {buildIndex} is not in the build settings.");
                return;
            }

            SceneManager.LoadScene(buildIndex);
        }
    }
}
