using System.Threading;
using System.Threading.Tasks;

namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>
    /// The only way to switch scenes, instead of calling the Unity SceneManager from
    /// different places. A transition shows the Loader curtain scene, loads the target
    /// scene additively while reporting progress and unloads the previous content scene.
    /// </summary>
    public interface IGameSceneManager
    {
        /// <summary>
        /// Runs the full transition to the scene. Completes when the target scene is active
        /// and the previous content scene and the loader scene are unloaded.
        /// </summary>
        Task Load(SceneName sceneName, CancellationToken cancellationToken = default);
    }
}
