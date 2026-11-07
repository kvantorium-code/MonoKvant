namespace AngryBirds.Core.Loading
{
    /// <summary>
    /// View contract used by the scene loader to report transition progress.
    /// Kept in Core so both Infrastructure and Ui can depend on it.
    /// </summary>
    public interface ILoadingScreen
    {
        void SetProgress(float progress);

        void SetStatus(string status);
    }
}
