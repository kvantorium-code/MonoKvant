using UnityEngine;
using VContainer;
using VContainer.Unity;
using AngryBirds.Core.Level;
using AngryBirds.Infrastructure.Flow;
using AngryBirds.Infrastructure.Input;

namespace AngryBirds.Infrastructure.Entry
{
    /// <summary>
    /// Project-level composition root: services that live for the whole application.
    /// Lives on a prefab placed in the Boot scene and survives scene loads.
    /// Scene-level scopes use it as their parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameInput gameInputPrefab;
        [SerializeField] private LevelCatalog levelCatalog;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            // Infrastructure services.
            builder.Register<GameSceneManager>(Lifetime.Singleton).As<IGameSceneManager>();
            builder.RegisterComponentInNewPrefab(gameInputPrefab, Lifetime.Singleton)
                .DontDestroyOnLoad()
                .As<IGameInput>();

            // Game data.
            builder.RegisterInstance<ILevelCatalog>(levelCatalog);

            // Game flow.
            builder.Register<GameStateMachine>(Lifetime.Singleton).As<IGameStateMachine>();
            builder.Register<BootState>(Lifetime.Singleton);
            builder.Register<MainMenuState>(Lifetime.Singleton);
            builder.Register<GameplayState>(Lifetime.Singleton);
            builder.Register<ResultState>(Lifetime.Singleton);

            // Further project-wide services (level catalog, progress, UI, localization) go here.

            builder.RegisterEntryPoint<AppEntryPoint>();
        }
    }
}
