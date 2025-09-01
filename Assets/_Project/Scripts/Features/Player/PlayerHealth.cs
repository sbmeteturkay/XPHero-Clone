using System;
using UnityEngine;
using Zenject;
using Game.Core.Interfaces;
using Game.Core.Services;
using UniRx;
using Game.Feature.UI;
using Unity.Mathematics;

namespace Game.Feature.Player
{
    public class PlayerHealth : IDamageable, IInitializable, ITickable, IDisposable
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private PlayerService playerService;
        [Inject]private IHealthBarRegistry _healthBarRegistry;
        
        private readonly ReactiveProperty<float> _maxHealth = new(100f);
        private readonly ReactiveProperty<float> _currentHealth = new(100f);
        private float _hpRecovery=1;
        private HealthBarController _healthBarController;
        float damageDisplayTimer;
        public void Initialize()
        {
            playerService.SetPlayerHealth(this);
            
            playerService.PlayerUpgradeData.Subscribe(OnUpgradeDataChanged);
            OnUpgradeDataChanged(playerService.PlayerUpgradeData.Value);
            
            _currentHealth.Value = _maxHealth.Value;
            _healthBarController = new HealthBarController(_healthBarRegistry, playerService.PlayerTransform,
                _currentHealth, _maxHealth, () => _currentHealth.Value < _maxHealth.Value);
        }

        public void Tick()
        {
            _healthBarController.Tick();
            if (damageDisplayTimer <= 0 && _currentHealth.Value < _maxHealth.Value)
            {
                damageDisplayTimer = 0;
                Heal();
            }
            else 
                damageDisplayTimer-= Time.deltaTime;
        }

        public void TakeDamage(float amount, GameObject instigator = null)
        {
            _currentHealth.Value -= amount;
            damageDisplayTimer = 3;
            
            //Debug.Log($"Oyuncu hasar aldı: {amount}. Kalan sağlık: {_currentHealth.Value}");

            //_signalBus.Fire(new PlayerTookDamageSignal { Player = this, DamageAmount = amount, Instigator = instigator });

            if (_currentHealth.Value <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            //Debug.Log("Oyuncu öldü.");
            //_signalBus.Fire(new PlayerDiedSignal { Player = this });
        }

        private void Heal()
        {
            _currentHealth.Value = Mathf.Clamp(_currentHealth.Value+(( _maxHealth.Value / 10000)*_hpRecovery), 0, _maxHealth.Value);
        }

        private void OnUpgradeDataChanged(UpgradeData upgradeData)
        {
           _maxHealth.Value = upgradeData.HP;
           _hpRecovery = math.remap(20,500,1,4,upgradeData.HPRecovery);
        }

        public void Dispose()
        {
            playerService?.Dispose();
            _maxHealth?.Dispose();
            _currentHealth?.Dispose();
        }
    }

    // Zenject Signal Tanımları
    public class PlayerTookDamageSignal { public PlayerHealth Player; public float DamageAmount; public GameObject Instigator; }
    public class PlayerDiedSignal { public PlayerHealth Player; }
    public class PlayerHealedSignal { public PlayerHealth Player; public float HealAmount; }
}

