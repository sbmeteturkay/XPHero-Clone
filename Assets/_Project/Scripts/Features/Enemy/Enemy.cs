using System;
using System.Collections;
using UnityEngine;
using Zenject;
using Game.Core.Interfaces;
using Game.Core.Services;
using Game.Feature.Spawn;
using R3;
using UnityEngine.AI;

namespace Game.Feature.Enemy
{
    public class Enemy : MonoBehaviour, IDamageable, IPoolable<EnemyData, IMemoryPool>
    {
        public Animator animator;
        [SerializeField] NavMeshAgent agent;
        [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
        private MaterialPropertyBlock materialPropertyBlock ;

        private EnemyData _enemyData;
        public EnemyData Data => _enemyData;

        private float _currentHealth;
        public float CurrentHealthPercent => _currentHealth/Data.MaxHealth;
        private IMemoryPool _pool; // Havuzu tutmak için
        private SpawnPoint _spawnPoint;
        
        [Inject] private SignalBus _signalBus; // Zenject SignalBus
        [Inject] public PlayerService playerService; // Oyuncuyu bulmak için
        public EnemyMovement EnemyMovement; 
        
        private EnemyStateController enemyStateController;
        public bool CanChaseOrAttack => _spawnPoint.PlayerInsideSpawnPoint;
        public ReactiveProperty<bool> CanSpawn = new (false);
        public IDisposable CanSpawnSubscription { get; set; }
        public bool Targetable=> isActiveAndEnabled && CanChaseOrAttack&&_currentHealth>0;
        
        private void Initialize()
        {
            enemyStateController = new(this);
            EnemyMovement = new EnemyMovement(agent,this);
            enemyStateController.Initialize();
            materialPropertyBlock = new();
            skinnedMeshRenderer.GetPropertyBlock(materialPropertyBlock);
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
            animator.CrossFade(IEnemyState.AnimNames.GetHit,0f);
            Flash();
            _currentHealth -= amount;
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        public void Flash()
        {

            materialPropertyBlock.SetFloat("_HitFlashAmount", .5f); // anlık beyaz
            skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);

            StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float amount = Mathf.Lerp(1f, 0f, t / 0.2f);
                materialPropertyBlock.SetFloat("_HitFlashAmount", amount);
                skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
                yield return null;
            }
        }

        private void Die()
        {
            _spawnPoint.EnemyDied(this);
            enemyStateController.TransitionToDeactivate();
            // Object Pool'a geri dönme
            //pool.Despawn(this);
        }

        // IPoolable arayüzü implementasyonu
        public void OnSpawned(EnemyData data, IMemoryPool pool)
        {
            _pool = pool;
            CanSpawn.Value = false;
            _enemyData = data;
            _currentHealth = _enemyData.MaxHealth;
            gameObject.SetActive(false);
            Initialize();
        }
        public void Respawn(EnemyData data)
        {
            CanSpawn.Value = false;
            _enemyData = data;
            _currentHealth = _enemyData.MaxHealth;
            gameObject.SetActive(true);
            enemyStateController.TransitionToIdle();
            agent.enabled = true;
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