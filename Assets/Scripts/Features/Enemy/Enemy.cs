using UnityEngine;
using Zenject;
using Game.Core.Interfaces;
using Game.Core.Services;

namespace Game.Feature.Enemy
{
    public class Enemy : MonoBehaviour, IDamageable, IPoolable<EnemyData, IMemoryPool>
    {
        [Inject] private DamageService _damageService;
        [Inject] private SignalBus _signalBus; // Zenject SignalBus

        private EnemyData _enemyData;
        private float _currentHealth;
        private IMemoryPool _pool; // Havuzu tutmak için

        public EnemyData Data => _enemyData;

        public void TakeDamage(float amount, GameObject instigator = null)
        {
            _currentHealth -= amount;
            Debug.Log($"{_enemyData.EnemyName} hasar aldı: {amount}. Kalan sağlık: {_currentHealth}");

            _signalBus.Fire(new EnemyTookDamageSignal { Enemy = this, DamageAmount = amount });

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"{_enemyData.EnemyName} öldü.");
            _signalBus.Fire(new EnemyDiedSignal { Enemy = this });
            // Object Pool'a geri dönme
            _pool.Despawn(this);
        }

        // Zenject MemoryPool için Factory metodu
        public class Factory : PlaceholderFactory<EnemyData, Enemy>
        {
        }

        // IPoolable arayüzü implementasyonu
        public void OnSpawned(EnemyData data, IMemoryPool pool)
        {
            _pool = pool;
            _enemyData = data;
            _currentHealth = _enemyData.MaxHealth;
            gameObject.SetActive(true);
            Debug.Log($"{_enemyData.EnemyName} havuzdan alındı (spawned).");
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
            _pool = null;
            Debug.Log($"{_enemyData.EnemyName} havuza geri döndü (despawned).");
        }
    }

    // Zenject Signal Tanımları
    public class EnemyTookDamageSignal
    {
        public Enemy Enemy;
        public float DamageAmount;
    }

    public class EnemyDiedSignal
    {
        public Enemy Enemy;
    }
}