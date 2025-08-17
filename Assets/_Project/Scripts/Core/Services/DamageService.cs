// Scripts/Core/Services/DamageService.cs
using UnityEngine;
using Zenject;
using Game.Core.Interfaces;

namespace Game.Core.Services
{
    public class DamageService
    {
        [Inject] private SignalBus _signalBus;

        public void ApplyDamage(IDamageable target, float amount, GameObject instigator = null)
        {
            if (target != null)
            {
                target.TakeDamage(amount, instigator);
                //_signalBus.Fire(new DamageAppliedSignal { Target = target, Amount = amount, Instigator = instigator });
            }
        }

        public void ApplyDamage(GameObject targetObject, float amount, GameObject instigator = null)
        {
            IDamageable target = targetObject.GetComponent<IDamageable>();
            if (target != null)
            {
                ApplyDamage(target, amount, instigator);
            }
            else
            {
                Debug.LogWarning($"GameObject {targetObject.name} IDamageable arayüzünü implemente etmiyor.");
            }
        }
    }

    // Zenject Signal Tanımı
    public class DamageAppliedSignal 
    { 
        public IDamageable Target; 
        public float Amount; 
        public GameObject Instigator; 
    }
}