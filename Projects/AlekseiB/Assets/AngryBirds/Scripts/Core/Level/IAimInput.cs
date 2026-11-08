using System;
using UnityEngine;

namespace AngryBirds.Core.Level
{
    /// <summary>
    /// What the slingshot controls need from the player input, as plain values and events.
    /// Implemented by the infrastructure input service, so Core never knows about devices.
    /// </summary>
    public interface IAimInput
    {
        /// <summary>Screen position of the pointer.</summary>
        Vector2 PointerPosition { get; }

        /// <summary>Direction the gamepad stick pulls the bird, zero while the stick rests.</summary>
        Vector2 StickDirection { get; }

        /// <summary>True while the gamepad stick drives the aim instead of the pointer.</summary>
        bool IsGamepadAim { get; }

        /// <summary>False on platforms without a pointer, aiming is then gamepad only.</summary>
        bool HasPointer { get; }

        event Action GrabStarted;

        event Action GrabReleased;

        event Action CancelPressed;
    }
}
