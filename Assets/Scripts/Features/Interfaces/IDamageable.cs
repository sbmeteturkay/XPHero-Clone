using UnityEngine;

namespace Game.Core.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(float amount, GameObject instigator = null);
    }
}