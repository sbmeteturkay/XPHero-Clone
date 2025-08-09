using System;
using UnityEngine;
using Zenject;
using Game.Core.Interfaces;
using Game.Core.Services;
using Game.Feature.Spawn;
using R3;

namespace Game.Feature.Enemy
{
    public class Enemy : MonoBehaviour, IDamageable, IPoolable<EnemyData, IMemoryPool>
    {
        public Animator animator;

        private EnemyData _enemyData;
        public EnemyData Data => _enemyData;

        private float _currentHealth;
        private IMemoryPool _pool; // Havuzu tutmak için
        private SpawnPoint _spawnPoint;
        
        [Inject] private DamageService _damageService;
        [Inject] private SignalBus _signalBus; // Zenject SignalBus
        [Inject] public PlayerService playerService; // Oyuncuyu bulmak için
        [Inject] public EnemyMovement EnemyMovement; // Oyuncuyu bulmak için
        [Inject] public EnemyAttack EnemyAttack; // Oyuncuyu bulmak için
        
        private EnemyStateController enemyStateController;
        public bool CanChaseOrAttack => _spawnPoint.EnemyCanChaseOrAttack;
        public ReactiveProperty<bool> CanSpawn = new (false);
        public IDisposable CanSpawnSubscription { get; set; }
        public bool Targetable
        {
            get { return isActiveAndEnabled && CanChaseOrAttack; }
        }

        private void Awake()
        {
            enemyStateController = new(this);
            enemyStateController.Initialize();
        }

        private void Update()
        {
            enemyStateController.Tick();
        }

        public void UpdateWhileDeactivated()
        {
            enemyStateController.Tick();
        }
        
        public void TakeDamage(float amount, GameObject instigator = null)
        {
            if (_currentHealth <= 0)
                return;
            _currentHealth -= amount;
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        private void Die()
        {
            _spawnPoint.EnemyDied(this);
            gameObject.SetActive(false);
            enemyStateController.TransitionToDeactivate();
            // Object Pool'a geri dönme
            //pool.Despawn(this);
        }

        // IPoolable arayüzü implementasyonu
        public void OnSpawned(EnemyData data, IMemoryPool pool)
        {
            Respawn(data);
            _pool = pool;
        }
        public void Respawn(EnemyData data)
        {
            CanSpawn.Value = false;
            _enemyData = data;
            _currentHealth = _enemyData.MaxHealth;
            gameObject.SetActive(true);
            enemyStateController.TransitionToIdle();
        }

        public void OnDespawned()
        {
        }

        public void SetSpawnPoint(SpawnPoint spawnPoint)
        {
            _spawnPoint = spawnPoint;
        }

        public Vector3 GetRandomPosition()
        {
            return _spawnPoint.GetRandomPositionInRadius();
        }
        // Zenject MemoryPool için Factory metodu
        public class Factory : PlaceholderFactory<EnemyData, Enemy>
        {
        }

    }

    // Zenject Signal Tanımları
    public class EnemyTookDamageSignal
    {
        public Enemy Enemy;
        public float DamageAmount;
    }

}