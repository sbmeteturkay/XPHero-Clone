using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using Zenject;
using Game.Core.Interfaces;
using Game.Core.Services;
using Game.Feature.Spawn;
using PrimeTween;
using R3;
using UI.PopupSystem;
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

        private float _currentHealth;
        private float CurrentHealthPercent => _currentHealth / Data.MaxHealth;
        private SpawnPoint _spawnPoint;
        
        private EnemyStateController enemyStateController;
        public bool CanChaseOrAttack => _spawnPoint.PlayerInsideSpawnPoint||isInteractingWithPlayer;
        public bool isInteractingWithPlayer;
        public ReactiveProperty<bool> CanSpawn = new(false);
        public IDisposable CanSpawnSubscription { get; set; }
        public bool Targetable => isActiveAndEnabled && CanChaseOrAttack && _currentHealth > 0;
        
        // Health bar gösterim kontrolü
        private readonly HealthBarData healthBar = new();
        private bool hasHealthBarRegistered = false; // Health bar durumu
        private bool shouldShowHealthBar = false;
        
        private void Initialize()
        {
            enemyStateController = new(this);
            EnemyMovement = new EnemyMovement(agent, this);
            enemyStateController.Initialize();
            materialPropertyBlock = new();
            skinnedMeshRenderer.GetPropertyBlock(materialPropertyBlock);
            
            // Health'i initialize et
            healthBar.maxHealth = Data.MaxHealth;
            healthBar.currentHealth = _currentHealth;
            
            // Health bar durumunu reset et
            hasHealthBarRegistered = false;
            shouldShowHealthBar = false;
        }

        private void Update()
        {
            enemyStateController.Tick();
            healthBar.UpdateTimer(Time.deltaTime);
            
            // Health bar yönetimi
            HandleHealthBarDisplay();
        }
        
        private void HandleHealthBarDisplay()
        {
            if (ShouldShowHealthBar())
            {
                UpdateHealthBar();
            }
            else if (hasHealthBarRegistered)
            {
                UnregisterHealthBar();
            }
        }

        private bool ShouldShowHealthBar()
        {
            return shouldShowHealthBar && 
                   _currentHealth > 0 && 
                   _currentHealth < Data.MaxHealth;
        }

        private void UpdateHealthBar()
        {
            Vector3 currentPosition = transform.position;
            float currentHealthPercent = healthBar.GetHealthPercent();
            float currentDamagePercent = healthBar.GetDamagePercent();
            if (!hasHealthBarRegistered)
            {
                RegisterHealthBar(currentPosition, currentHealthPercent, currentDamagePercent);
            }
            UpdateHealthBarPositionAndValues(currentPosition, currentHealthPercent, currentDamagePercent);
        }

        private void RegisterHealthBar(Vector3 position, float healthPercent, float damagePercent)
        {
            if (healthBarService.RegisterHealthBar(gameObject, position, healthPercent, damagePercent))
            {
                hasHealthBarRegistered = true;
            }
        }
        
        private void UpdateHealthBarPositionAndValues(Vector3 position, float healthPercent, float damagePercent)
        {
            healthBarService.UpdateHealthBar(gameObject, position, healthPercent, damagePercent);
        }
        
        private void UnregisterHealthBar()
        {
            if (hasHealthBarRegistered)
            {
                healthBarService.UnregisterHealthBar(gameObject);
                hasHealthBarRegistered = false;
            }
        }

        public void UpdateWhileDeactivated()
        {
            enemyStateController.Tick();
        }
        
        public void TakeDamage(float amount, GameObject instigator = null)
        {
            if (_currentHealth <= 0)
                return;
                
            //animator.CrossFade(IEnemyState.AnimNames.GetHit, 0f);
            
            Flash();
            KnockBack();
            _currentHealth -= amount;
            healthBar.TakeDamage(amount);
            
            // Health bar'ı göstermeye başla
            shouldShowHealthBar = true;
            
            _popupManager.ShowDamage(transform.position+Vector3.up*1.5f,amount.ToString(CultureInfo.InvariantCulture),Color.white);
            if (_currentHealth <= 1)
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
            _currentHealth += _enemyData.MaxHealth / 1000;
            _currentHealth= Mathf.Clamp(_currentHealth, 0, Data.MaxHealth);
            shouldShowHealthBar = _currentHealth < _enemyData.MaxHealth;
            healthBar.currentHealth = _currentHealth;
        }

        private void Flash()
        {
            materialPropertyBlock.SetFloat("_HitFlashAmount", .5f);
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
            // Health bar'ı hemen kaldır
            UnregisterHealthBar();
            shouldShowHealthBar = false;
            
            _spawnPoint.EnemyDied(this);
            enemyStateController.TransitionToDeactivate();
        }

        public void OnSpawned(EnemyData data, IMemoryPool pool)
        {
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
            healthBar.currentHealth = _currentHealth;
            
            // Health bar durumunu reset et
            shouldShowHealthBar = false;
            UnregisterHealthBar();
            
            gameObject.SetActive(true);
            enemyStateController.TransitionToIdle();
            agent.enabled = true;
        }

        public void OnDespawned()
        {
            // Pool'a dönerken health bar'ı temizle
            UnregisterHealthBar();
            shouldShowHealthBar = false;
        }

        public void SetSpawnPoint(SpawnPoint spawnPoint)
        {
            _spawnPoint = spawnPoint;
        }

        public Vector3 GetRandomPosition()
        {
            return _spawnPoint.GetRandomPositionInRadius();
        }
        
        // Component destroy olurken health bar'ı temizle
        private void OnDestroy()
        {
            UnregisterHealthBar();
        }
        
        // GameObject deaktif olurken health bar'ı temizle
        private void OnDisable()
        {
            UnregisterHealthBar();
            shouldShowHealthBar = false;
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