namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>The only way to switch scenes, instead of calling SceneManager from different places.</summary>
    public interface ISceneLoader
    {
        bool CanLoad(string sceneName);

        bool CanLoad(int buildIndex);

        void Load(string sceneName);

        void Load(int buildIndex);
    }
}
