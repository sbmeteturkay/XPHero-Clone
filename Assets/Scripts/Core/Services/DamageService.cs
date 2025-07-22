using Game.Core.Interfaces;
using UnityEngine;

namespace Game.Core.Services
{
    public class DamageService
    {
        // EventBus aracılığıyla hasar olaylarını yayınlamak için inject edilebilir.
        // [Inject] private EventBus _eventBus;

        public void ApplyDamage(IDamageable target, float amount, GameObject instigator = null)
        {
            if (target == null)
            {
                Debug.LogWarning("Hasar uygulanacak hedef null.");
                return;
            }

            target.TakeDamage(amount, instigator);

            // Hasar olayını yayınla (örneğin, UI'da hasar sayısını göstermek için)
            // _eventBus.Publish(new DamageTakenEvent { Target = target, Amount = amount, Instigator = instigator });
        }

        // Kritik vuruş, zırh hesaplamaları gibi ek mantıklar buraya eklenebilir.
    }
}