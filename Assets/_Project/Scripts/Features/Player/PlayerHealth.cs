// Scripts/Feature/Player/PlayerHealth.cs

using System;
using UnityEngine;
using Zenject;
using Game.Core.Interfaces;
using Game.Core.Services;
using UniRx;

namespace Game.Feature.Player
{
    public class PlayerHealth :  IDamageable, IInitializable
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private PlayerService playerService;

        private float _maxHealth = 100f;
        private float _currentHealth=100f;

        public void TakeDamage(float amount, GameObject instigator = null)
        {
            _currentHealth -= amount;
            //Debug.Log($"Oyuncu hasar aldı: {amount}. Kalan sağlık: {_currentHealth}");

            //_signalBus.Fire(new PlayerTookDamageSignal { Player = this, DamageAmount = amount, Instigator = instigator });

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            //Debug.Log("Oyuncu öldü.");
            //_signalBus.Fire(new PlayerDiedSignal { Player = this });
        }

        public void Heal(float amount)
        {
            _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
            Debug.Log($"Oyuncu iyileşti: {amount}. Yeni sağlık: {_currentHealth}");
            _signalBus.Fire(new PlayerHealedSignal { Player = this, HealAmount = amount });
        }

        public void Initialize()
        {
            playerService.PlayerDamageable = this;
            playerService.PlayerUpgradeData.Subscribe(OnUpgradeDataChanged);
            OnUpgradeDataChanged(playerService.PlayerUpgradeData.Value);
            _currentHealth = _maxHealth;
        }
        private void OnUpgradeDataChanged(UpgradeData upgradeData)
        {
           _maxHealth = upgradeData.HP;
        }
    }

    // Zenject Signal Tanımları
    public class PlayerTookDamageSignal { public PlayerHealth Player; public float DamageAmount; public GameObject Instigator; }
    public class PlayerDiedSignal { public PlayerHealth Player; }
    public class PlayerHealedSignal { public PlayerHealth Player; public float HealAmount; }
}