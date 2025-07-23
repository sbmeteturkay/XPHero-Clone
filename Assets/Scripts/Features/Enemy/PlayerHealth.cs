// Scripts/Feature/Player/PlayerHealth.cs
using UnityEngine;
using Zenject;
using Game.Core.Interfaces;
using UniRx;

namespace Game.Feature.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Inject] private SignalBus _signalBus;

        [SerializeField] private float _maxHealth = 100f;
        private ReactiveProperty<float> _currentHealth = new ReactiveProperty<float>();

        public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;

        void Awake()
        {
            _currentHealth.Value = _maxHealth;
        }

        public void TakeDamage(float amount, GameObject instigator = null)
        {
            _currentHealth.Value -= amount;
            Debug.Log($"Oyuncu hasar aldı: {amount}. Kalan sağlık: {_currentHealth.Value}");

            //_signalBus.Fire(new PlayerTookDamageSignal { Player = this, DamageAmount = amount, Instigator = instigator });

            if (_currentHealth.Value <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Oyuncu öldü.");
            _signalBus.Fire(new PlayerDiedSignal { Player = this });
            // Oyun bitiş ekranı, yeniden başlama vb. mantık buraya eklenebilir.
            gameObject.SetActive(false);
        }

        public void Heal(float amount)
        {
            _currentHealth.Value = Mathf.Min(_currentHealth.Value + amount, _maxHealth);
            Debug.Log($"Oyuncu iyileşti: {amount}. Yeni sağlık: {_currentHealth.Value}");
            _signalBus.Fire(new PlayerHealedSignal { Player = this, HealAmount = amount });
        }
    }

    // Zenject Signal Tanımları
    public class PlayerTookDamageSignal { public PlayerHealth Player; public float DamageAmount; public GameObject Instigator; }
    public class PlayerDiedSignal { public PlayerHealth Player; }
    public class PlayerHealedSignal { public PlayerHealth Player; public float HealAmount; }
}