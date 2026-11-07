using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace AngryBirds.Infrastructure
{
    /// <summary>
    /// Loads scenes by <see cref="SceneName"/> through the dedicated loader scene.
    /// The loader scene is shown on top of the current scene, reports progress and is
    /// unloaded automatically when the target scene activates.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SceneManager : MonoBehaviour
    {
        private const string LoaderSceneName = "Loader";

        private static SceneManager _instance;
        private static AsyncOperation _activeOperation;
        private static bool _isLoading;

        /// <summary>Normalized progress (0..1) of the scene that is currently loading.</summary>
        public static float Progress { get; private set; }

        /// <summary>True while a load started by this service is in progress.</summary>
        public static bool IsLoading => _isLoading;

        private static SceneManager Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // The host must be a root object so it survives scene changes.
                var host = new GameObject("[SceneManager]");
                _instance = host.AddComponent<SceneManager>();
                return _instance;
            }
        }

        /// <summary>Starts loading <paramref name="sceneName"/> through the loader scene.</summary>
        public static void Load(SceneName sceneName)
        {
            LoadAsync(sceneName);
        }

        /// <summary>
        /// Starts loading <paramref name="sceneName"/> through the loader scene and returns
        /// the underlying operation. The operation completes after the target scene activates.
        /// </summary>
        public static AsyncOperation LoadAsync(SceneName sceneName)
        {
            string targetSceneName = sceneName.ToSceneName();

            if (targetSceneName == LoaderSceneName)
            {
                Debug.LogError("[SceneManager] The loader scene cannot be loaded through itself.");
                return null;
            }

            if (_isLoading)
            {
                Debug.LogWarning($"[SceneManager] Scene '{sceneName}' was requested while another scene is loading. Request ignored.");
                return _activeOperation;
            }

            if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                Debug.LogError($"[SceneManager] Scene '{targetSceneName}' is not in the build settings.");
                return null;
            }

            if (!Application.CanStreamedLevelBeLoaded(LoaderSceneName))
            {
                Debug.LogWarning("[SceneManager] Loader scene is not in the build settings. Loading the target scene directly.");
                return UnitySceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
            }

            SceneManager instance = Instance;

            if (!UnitySceneManager.GetSceneByName(LoaderSceneName).isLoaded)
            {
                UnitySceneManager.LoadScene(LoaderSceneName, LoadSceneMode.Additive);
            }

            AsyncOperation operation = UnitySceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);

            if (operation == null)
            {
                Debug.LogError($"[SceneManager] Failed to start loading scene '{targetSceneName}'.");
                UnitySceneManager.UnloadSceneAsync(LoaderSceneName);
                return null;
            }

            // The loader scene stays visible while the target streams in.
            operation.allowSceneActivation = false;
            _activeOperation = operation;
            _isLoading = true;
            Progress = 0f;

            instance.StartCoroutine(instance.TrackLoad(operation, targetSceneName));
            return operation;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (transform.parent != null)
            {
                // A service that survives scene changes must live on a root object.
                var host = new GameObject("[SceneManager]");
                _instance = host.AddComponent<SceneManager>();
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private IEnumerator TrackLoad(AsyncOperation operation, string targetSceneName)
        {
            // Let the loader scene run Awake/Start before the first progress update.
            yield return null;

            ILoadingScreen screen = FindLoadingScreen();
            screen?.SetStatus($"LOADING {targetSceneName.ToUpperInvariant()}");
            screen?.SetProgress(0f);

            // Unity reports 0..0.9 while the scene streams in; scale it to 0..1.
            while (!operation.isDone && operation.progress < 0.9f)
            {
                SetProgress(operation.progress / 0.9f, screen);
                yield return null;
            }

            SetProgress(1f, screen);

            // Keep the full bar on screen for one frame before activating the target.
            yield return null;

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }

            _isLoading = false;
            _activeOperation = null;
        }

        private static void SetProgress(float progress, ILoadingScreen screen)
        {
            Progress = Mathf.Clamp01(progress);
            screen?.SetProgress(Progress);
        }

        private static ILoadingScreen FindLoadingScreen()
        {
            Scene loaderScene = UnitySceneManager.GetSceneByName(LoaderSceneName);

            if (!loaderScene.isLoaded)
            {
                return null;
            }

            foreach (GameObject root in loaderScene.GetRootGameObjects())
            {
                foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour is ILoadingScreen screen)
                    {
                        return screen;
                    }
                }
            }

            return null;
        }
    }
}
