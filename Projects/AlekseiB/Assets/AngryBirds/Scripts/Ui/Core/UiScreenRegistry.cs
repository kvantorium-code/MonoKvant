using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngryBirds.Ui
{
    /// <summary>
    /// Maps <see cref="UiScreenId"/> to the screen prefab. Lives on <see cref="UiRoot"/>,
    /// new screens are added to the asset in the Inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "AngryBirds/Ui Screen Registry", fileName = "UiScreenRegistry")]
    public sealed class UiScreenRegistry : ScriptableObject
    {
        [SerializeField] private List<Entry> screens = new List<Entry>();

        public IReadOnlyList<Entry> Screens => screens;

        [Serializable]
        public struct Entry
        {
            [SerializeField] private UiScreenId id;
            [SerializeField] private ScreenBase prefab;

            public UiScreenId Id => id;

            public ScreenBase Prefab => prefab;
        }
    }
}
