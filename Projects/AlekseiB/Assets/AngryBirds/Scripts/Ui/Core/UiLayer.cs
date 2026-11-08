namespace AngryBirds.Ui
{
    /// <summary>
    /// Canvas layers of <see cref="UiRoot"/>. Declaration order is the drawing order:
    /// a later layer is drawn above and receives input first.
    /// </summary>
    public enum UiLayer
    {
        Hud = 0,
        Screens = 1,
        Popups = 2,
        Overlay = 3
    }
}
