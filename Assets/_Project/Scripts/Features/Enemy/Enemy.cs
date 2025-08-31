using System;
using System.Globalization;
using UnityEngine;
using Zenject;
using Game.Core.Interfaces;
using Game.Core.Services;
using Game.Feature.Spawn;
using Game.Feature.UI;
using PrimeTween;
using UI.PopupSystem;
using UniRx;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Game.Feature.Enemy
{
    public class Enemy : MonoBehaviour, IDamageable, IPoolable<EnemyData, IMemoryPool>
    {
        [Inject] private SignalBus _signalBus;
        [Inject] public DamageService damageService;
        [Inject] public PlayerService playerService;
        [Inject] private IHealthBarRegistry healthBarService;
        [Inject] private PopupManager _popupManager;
        
        public Animator animator;
        [SerializeField] NavMeshAgent agent;
        [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
        private MaterialPropertyBlock materialPropertyBlock;
        
        public EnemyMovement EnemyMovement; 
        private EnemyData _enemyData;
        public EnemyData Data => _enemyData;

        private readonly ReactiveProperty<float> _currentHealth=new();
        private SpawnPoint _spawnPoint;
        
        private EnemyStateController enemyStateController;
        public bool CanChaseOrAttack => _spawnPoint.PlayerInsideSpawnPoint||isInteractingWithPlayer;
        public bool isInteractingWithPlayer;
        public readonly ReactiveProperty<bool> CanSpawn = new(false);
        public IDisposable CanSpawnSubscription { get; set; }
        public bool Targetable => isActiveAndEnabled && CanChaseOrAttack && _currentHealth.Value > 0;
        
        // Health bar gösterim kontrolü
        private HealthBarController _healthBarController;
        
        private void Initialize()
        {
            enemyStateController = new(this);
            EnemyMovement = new EnemyMovement(agent, this);
            enemyStateController.Initialize();
            materialPropertyBlock = new();
            skinnedMeshRenderer.GetPropertyBlock(materialPropertyBlock);
            
            // Health bar durumunu reset et
            _healthBarController = new HealthBarController(healthBarService,transform,
                _currentHealth, 
                new ReactiveProperty<float>(Data.MaxHealth),
                ()=>_currentHealth.Value > 1 && _currentHealth.Value < Data.MaxHealth);
        }

        private void Update()
        {
            enemyStateController.Tick();
            _healthBarController.Tick();
        }
        
        public void UpdateWhileDeactivated()
        {
            enemyStateController.Tick();
        }
        
        public void TakeDamage(float amount, GameObject instigator = null)
        {
            if (_currentHealth.Value <= 0)
                return;
                
            //animator.CrossFade(IEnemyState.AnimNames.GetHit, 0f);
            
            Flash();
            KnockBack();
            _currentHealth.Value -= amount;
            
            _popupManager.ShowDamage(transform.position+Vector3.up*1.5f,amount.ToString(CultureInfo.InvariantCulture),Color.white);
            if (_currentHealth.Value <= 1)
            {
                Die();
            }
        }

        private void KnockBack()
        {
            Vector3 startPos = transform.position;
            Vector3 hitDirection = (playerService.GetPlayerPosition() - transform.position).normalized;
            // Hangi yöne zıplatılacağı
            Vector3 targetPos = startPos - hitDirection.normalized * .75f;

            // Hedef pozisyona git, ease-outbounce ile
            Tween.Position(transform, targetPos, .2f, (Ease)Random.Range(1,31));

            // Y için ayrı tween: sadece yukarı zıplama ve iniş
            Tween.LocalPositionY(transform, startPos.y + .75f, .2f / 2, Ease.Linear, cycles: 2,
                cycleMode: CycleMode.Yoyo);
        }

        public void Heal()
        {
            _currentHealth.Value= Mathf.Clamp(_currentHealth.Value+(_enemyData.MaxHealth / 1000), 0, Data.MaxHealth);
        }

        private void Flash()
        {
            Color startColor = Color.white*1.5f; // parlaklık başlangıcı
            Color endColor = Color.black;        // sona erme

            Tween.Custom(startColor, endColor, 0.4f, ( val) =>
            {
                materialPropertyBlock.SetColor("_EmissionColor", val);
                skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
            });
        }

        private void Die()
        {
            // Health bar'ı hemen kaldır
            _spawnPoint.EnemyDied(this);
            enemyStateController.TransitionToDeactivate();
        }

        public void OnSpawned(EnemyData data, IMemoryPool pool)
        {
            CanSpawn.Value = false;
            _enemyData = data;
            _currentHealth.Value = Data.MaxHealth;
            gameObject.SetActive(false);
            Initialize();
        }
        
        public void Respawn(EnemyData data)
        {
            CanSpawn.Value = false;
            _enemyData = data;
            _currentHealth.Value = Data.MaxHealth;
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

        public float DistanceFromSpawnPoint()
        {
            return Vector3.Distance(_spawnPoint.transform.position, transform.position);
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