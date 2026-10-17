using UnityEngine;

namespace AngryBirds.Core.Damage
{
    /// <summary>
    /// Anything that has hit points and can be destroyed.
    /// Implemented by actors that take part in collisions (birds, blocks, pigs).
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }

        float HealthNormalized { get; }

        void ApplyDamage(DamageInfo info);
    }
}
