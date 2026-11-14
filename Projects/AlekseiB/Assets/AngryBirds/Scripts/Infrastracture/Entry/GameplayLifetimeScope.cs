using UnityEngine;
using VContainer;
using VContainer.Unity;
using AngryBirds.Core.Level;
using AngryBirds.Ui;

namespace AngryBirds.Infrastructure.Entry
{
    /// <summary>
    /// Scene-level composition root of the Gameplay scene. Its parent is the project scope.
    /// When the scene is opened directly (the Boot scene was not loaded) there is no project
    /// scope yet, so the scene creates one from the fallback prefab.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private ProjectLifetimeScope fallbackProjectScope;

        [Header("Scene objects")]
        [SerializeField] private LevelSpawner levelSpawner;
        [SerializeField] private ShotFlow shotFlow;
        [SerializeField] private AimController aimController;
        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private HudView hud;

        protected override LifetimeScope FindParent()
        {
            LifetimeScope project = Find<ProjectLifetimeScope>();

            if (project == null && fallbackProjectScope != null)
            {
                project = Instantiate(fallbackProjectScope);
            }

            return project;
        }

        protected override void Configure(IContainerBuilder builder)
        {
            // The runtime state of the level being played. The scene is reloaded for every level,
            // so a fresh instance is created each time.
            builder.Register<LevelRuntime>(Lifetime.Singleton);
            builder.Register<HudController>(Lifetime.Singleton);

            builder.RegisterComponent(levelSpawner);
            builder.RegisterComponent(shotFlow);
            builder.RegisterComponent(aimController);
            builder.RegisterComponent(cameraRig);
            builder.RegisterComponent(hud);

            builder.RegisterEntryPoint<GameplayEntryPoint>();
        }
    }
}
