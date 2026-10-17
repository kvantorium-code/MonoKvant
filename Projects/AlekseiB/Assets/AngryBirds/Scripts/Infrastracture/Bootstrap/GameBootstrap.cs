using UnityEngine;
using UnityEngine.SceneManagement;
using AngryBirds.Core.Gameplay;
using AngryBirds.Core.Level;
using AngryBirds.Infrastructure.Input;
using AngryBirds.Ui;

namespace AngryBirds.Infrastructure.Bootstrap
{
    /// <summary>
    /// Composition root of a level scene.
    /// Creates the level runtime, injects it into the director, the camera and the HUD,
    /// translates input into slingshot calls and owns scene navigation.
    /// Contains no gameplay rules, it only wires parts together.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private GameInput input;
        [SerializeField] private LevelDirector director;
        [SerializeField] private Slingshot slingshot;
        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private HudView hud;

        [Header("Flow")]
        [SerializeField] private string sceneNameFormat = "Level_{0:00}";
        [SerializeField] private string firstLevelScene = "Level_01";

        [Header("Pointer")]
        [SerializeField, Min(0.1f)] private float pointerGrabRadius = 1.75f;

        private LevelRuntime _runtime;
        private Camera _camera;
        private bool _isLevelFinished;

        private void Awake()
        {
            _runtime = new LevelRuntime();
            _camera = cameraRig != null ? cameraRig.GetComponent<Camera>() : null;

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            LevelDefinition level = director.Level;
            if (level == null)
            {
                Debug.LogError("[GameBootstrap] The level director has no level definition.", this);
                enabled = false;
                return;
            }

            director.Initialize(_runtime);
            cameraRig.Initialize(_runtime, level);
            hud.Initialize(_runtime, level);

            hud.LevelFinished += OnLevelFinished;
            hud.RestartRequested += RestartLevel;
            hud.NextLevelRequested += LoadNextLevel;

            input.GrabStarted += OnGrabStarted;
            input.GrabReleased += OnGrabReleased;
            input.CancelPressed += OnCancelPressed;
            input.RestartPressed += RestartLevel;
            input.NextLevelPressed += OnNextLevelPressed;
        }

        private void OnDestroy()
        {
            hud.LevelFinished -= OnLevelFinished;
            hud.RestartRequested -= RestartLevel;
            hud.NextLevelRequested -= LoadNextLevel;

            input.GrabStarted -= OnGrabStarted;
            input.GrabReleased -= OnGrabReleased;
            input.CancelPressed -= OnCancelPressed;
            input.RestartPressed -= RestartLevel;
            input.NextLevelPressed -= OnNextLevelPressed;
        }

        private void Update()
        {
            if (slingshot.IsAiming)
            {
                slingshot.UpdateAim(ResolveAimPoint());
            }
        }

        private Vector2 ResolveAimPoint()
        {
            if (input.IsGamepadAim)
            {
                return slingshot.AnchorPosition + input.StickDirection * slingshot.MaxPullDistance;
            }

            return PointerToWorld(input.PointerPosition);
        }

        /// <summary>Converts a screen-space pointer position into a world position on the play plane.</summary>
        private Vector2 PointerToWorld(Vector2 screenPosition)
        {
            if (_camera == null)
            {
                return screenPosition;
            }

            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_camera.transform.position.z));
            return world;
        }

        private void OnGrabStarted()
        {
            if (_isLevelFinished || !slingshot.HasBird)
            {
                return;
            }

            if (!input.IsGamepadAim)
            {
                if (!input.HasPointer)
                {
                    return;
                }

                // Pointer aiming only starts when the press is close to the loaded bird,
                // so UI buttons and stray clicks never grab the slingshot.
                Vector2 pointerWorld = PointerToWorld(input.PointerPosition);

                if (Vector2.Distance(pointerWorld, slingshot.AnchorPosition) > pointerGrabRadius)
                {
                    return;
                }
            }

            slingshot.BeginAim(ResolveAimPoint());
        }

        private void OnGrabReleased()
        {
            slingshot.ReleaseAim();
        }

        private void OnCancelPressed()
        {
            if (!slingshot.IsAiming)
            {
                return;
            }

            slingshot.CancelAim();
        }

        private void OnNextLevelPressed()
        {
            if (_isLevelFinished)
            {
                LoadNextLevel();
            }
        }

        private void OnLevelFinished(LevelOutcome outcome)
        {
            _isLevelFinished = true;
            slingshot.CancelAim();
            hud.ShowResult(outcome, outcome.IsWin ? "LEVEL CLEARED" : "TRY AGAIN", HasNextLevel());
        }

        private bool HasNextLevel()
        {
            return Application.CanStreamedLevelBeLoaded(GetSceneName(director.Level.LevelNumber + 1));
        }

        private void RestartLevel()
        {
            LoadScene(gameObject.scene.name);
        }

        private void LoadNextLevel()
        {
            string next = GetSceneName(director.Level.LevelNumber + 1);
            LoadScene(Application.CanStreamedLevelBeLoaded(next) ? next : firstLevelScene);
        }

        private string GetSceneName(int levelNumber)
        {
            return string.Format(sceneNameFormat, levelNumber);
        }

        private static void LoadScene(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[GameBootstrap] Scene '{sceneName}' is not in the build settings.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
