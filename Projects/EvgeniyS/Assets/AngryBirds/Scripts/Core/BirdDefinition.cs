using UnityEngine;

namespace AngryBirds.Core
{
    [CreateAssetMenu(fileName = "BirdDefinition", menuName = "AngryBirds/Bird Definition")]
    public sealed class BirdDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName = "Red";
        [SerializeField] private Color _color = Color.red;
        [SerializeField] private float _mass = 1f;
        [SerializeField] private float _radius = 0.35f;
        [SerializeField] private float _launchPower = 5f;
        [SerializeField] private float _damageMultiplier = 1f;

        public string DisplayName => _displayName;
        public Color Color => _color;
        public float Mass => _mass;
        public float Radius => _radius;
        public float LaunchPower => _launchPower;
        public float DamageMultiplier => _damageMultiplier;
    }
}
