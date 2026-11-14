using System.Threading;
using System.Threading.Tasks;
using AngryBirds.Core.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>
    /// Scene transitions behind <see cref="IGameSceneManager"/>. The only place in the project
    /// that calls the Unity SceneManager.
    ///
    /// A transition is fully additive so the curtain scene survives it: a Single load would
    /// unload the additive Loader scene together with the previous content.
    /// 1. the Loader scene is loaded additively and shows the loading screen;
    /// 2. the target scene is loaded additively with deferred activation, its streaming
    ///    progress is forwarded to the loading screen;
    /// 3. the target scene activates and becomes the active scene;
    /// 4. the previous content scene is unloaded;
    /// 5. after one frame for the new scene to initialize, the Loader scene is unloaded.
    ///
    /// The Boot scene (build index 0) is never unloaded by a transition, the project scope
    /// lives there for the whole application.
    /// </summary>
    public sealed class GameSceneManager : IGameSceneManager
    {
        private const string LoaderSceneName = "Loader";

        private Scene _contentScene;
        private bool _isLoading;

        public async Task Load(SceneName sceneName, CancellationToken cancellationToken = default)
        {
            string targetSceneName = sceneName.ToSceneName();

            if (targetSceneName == LoaderSceneName)
            {
                Debug.LogError("[GameSceneManager] The loader scene cannot be loaded through itself.");
                return;
            }

            if (_isLoading)
            {
                Debug.LogWarning($"[GameSceneManager] Scene '{targetSceneName}' was requested while another transition is running. Request ignored.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                Debug.LogError($"[GameSceneManager] Scene '{targetSceneName}' is not in the build settings.");
                return;
            }

            _isLoading = true;

            try
            {
                await Transition(targetSceneName, cancellationToken);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task Transition(string targetSceneName, CancellationToken cancellationToken)
        {
            Scene loaderScene = default;
            ILoadingScreen screen = null;

            if (Application.CanStreamedLevelBeLoaded(LoaderSceneName))
            {
                AsyncOperation loaderOperation = UnitySceneManager.LoadSceneAsync(LoaderSceneName, LoadSceneMode.Additive);

                if (loaderOperation != null)
                {
                    while (!loaderOperation.isDone)
                    {
                        await Awaitable.NextFrameAsync(cancellationToken);
                    }

                    loaderScene = UnitySceneManager.GetSceneAt(UnitySceneManager.sceneCount - 1);
                    screen = FindLoadingScreen(loaderScene);
                    screen?.SetStatus($"LOADING {targetSceneName.ToUpperInvariant()}");
                    screen?.SetProgress(0f);
                }
            }
            else
            {
                Debug.LogWarning("[GameSceneManager] Loader scene is not in the build settings. Loading without the curtain.");
            }

            AsyncOperation operation = UnitySceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);

            if (operation == null)
            {
                Debug.LogError($"[GameSceneManager] Failed to start loading scene '{targetSceneName}'.");
                await UnloadQuietly(loaderScene);
                return;
            }

            operation.allowSceneActivation = false;

            // Unity reports 0..0.9 while the scene streams in, scale it to the full bar.
            while (operation.progress < 0.9f)
            {
                screen?.SetProgress(operation.progress / 0.9f);
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            screen?.SetProgress(1f);

            // Keep the full bar on screen for one frame before the swap.
            await Awaitable.NextFrameAsync(cancellationToken);

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            Scene newScene = UnitySceneManager.GetSceneAt(UnitySceneManager.sceneCount - 1);
            UnitySceneManager.SetActiveScene(newScene);

            Scene previous = _contentScene;
            _contentScene = newScene;

            if (previous.IsValid() && previous.isLoaded)
            {
                await UnitySceneManager.UnloadSceneAsync(previous);
            }

            // Let the new scene run its entry points for a frame before the curtain goes away.
            await Awaitable.NextFrameAsync(cancellationToken);

            await UnloadQuietly(loaderScene);
        }

        private static async Task UnloadQuietly(Scene scene)
        {
            if (scene.IsValid() && scene.isLoaded)
            {
                await UnitySceneManager.UnloadSceneAsync(scene);
            }
        }

        private static ILoadingScreen FindLoadingScreen(Scene loaderScene)
        {
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
