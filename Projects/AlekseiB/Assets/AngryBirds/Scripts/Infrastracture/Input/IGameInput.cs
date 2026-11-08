using System;
using AngryBirds.Core.Level;

namespace AngryBirds.Infrastructure.Input
{
    /// <summary>
    /// Player input as plain values and events, independent of devices and action names.
    /// The slingshot part comes from the Core-side <see cref="IAimInput"/>, the rest is navigation.
    /// </summary>
    public interface IGameInput : IAimInput
    {
        event Action RestartPressed;

        event Action NextLevelPressed;
    }
}
