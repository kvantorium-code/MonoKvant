namespace AngryBirds.Ui
{
    /// <summary>
    /// Identifiers of the project screens. A prefab is bound to an id in <see cref="UiScreenRegistry"/>.
    /// Values are serialized in the registry asset, never renumber existing entries.
    /// </summary>
    public enum UiScreenId
    {
        None = 0,
        Hud = 1,
        LevelResult = 2,
        MainMenu = 3,
        LevelSelect = 4,
        Pause = 5,

        /// <summary>Temporary screen that proves the UI core works, removed together with the debug folder.</summary>
        DebugTest = 100
    }
}
