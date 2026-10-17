using UnityEngine;

namespace AngryBirds.Core.Damage
{
    /// <summary>
    /// Converts a physics contact impulse into gameplay damage.
    /// Impulse based, so a heavy object hurts more than a light one at the same speed
    /// and a fast object hurts more than a slow one of the same mass.
    /// </summary>
    public static class ImpactDamage
    {
        /// <summary>Impulse below this value is treated as a harmless touch.</summary>
        public const float MinImpulse = 2.5f;

        /// <summary>Damage per unit of impulse above the threshold.</summary>
        public const float DamagePerImpulse = 1.6f;

        /// <summary>Upper bound, keeps a single hit from destroying everything at once.</summary>
        public const float MaxDamagePerImpact = 400f;

        public static float Calculate(float contactImpulse)
        {
            if (contactImpulse <= MinImpulse)
            {
                return 0f;
            }

            return Mathf.Min((contactImpulse - MinImpulse) * DamagePerImpulse, MaxDamagePerImpact);
        }
    }
}
