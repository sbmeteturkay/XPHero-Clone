using System;
using System.Collections;
using UnityEngine;
using Zenject;
using Game.Core.Interfaces;
using Game.Core.Services;
using Game.Feature.Spawn;
using R3;
using UI.PopupSystem;
using UnityEngine.AI;

namespace Game.Feature.Enemy
{
    public class Enemy : MonoBehaviour, IDamageable, IPoolable<EnemyData, IMemoryPool>
    {
        private bool hasHealthBarRegistered = false; // Health bar durumu
        
        public Animator animator;
        [SerializeField] NavMeshAgent agent;
        [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
        private MaterialPropertyBlock materialPropertyBlock;

        private EnemyData _enemyData;
        public EnemyData Data => _enemyData;

        private float _currentHealth;
        public float CurrentHealthPercent => _currentHealth / Data.MaxHealth;
        private IMemoryPool _pool;
        private SpawnPoint _spawnPoint;
        
        [Inject] private SignalBus _signalBus;
        [Inject] public PlayerService playerService;
        [Inject] private HealthBarService healthBarService;
        [Inject] private PopupManager _popupManager;
        
        HealthBarData healthBar = new HealthBarData();
        
        public EnemyMovement EnemyMovement; 
        
        private EnemyStateController enemyStateController;
        public bool CanChaseOrAttack => _spawnPoint.PlayerInsideSpawnPoint;
        public ReactiveProperty<bool> CanSpawn = new(false);
        public IDisposable CanSpawnSubscription { get; set; }
        public bool Targetable => isActiveAndEnabled && CanChaseOrAttack && _currentHealth > 0;
        
        // Health bar gösterim kontrolü
        private bool shouldShowHealthBar = false;
        private Vector3 lastPosition;
        private float lastHealthPercent;
        private float lastDamagePercent;
        
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
            lastPosition = transform.position;
            lastHealthPercent = CurrentHealthPercent;
            lastDamagePercent = healthBar.GetDamagePercent();
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
            // Health bar gösterilmeli mi kontrol et
            bool shouldShow = shouldShowHealthBar && _currentHealth > 0 && _currentHealth < Data.MaxHealth;
            
            if (shouldShow)
            {
                if (!hasHealthBarRegistered)
                {
                    // Health bar'ı register et
                    if (healthBarService.RegisterHealthBar(
                        gameObject,
                        transform.position,
                        CurrentHealthPercent,
                        healthBar.GetDamagePercent()))
                    {
                        hasHealthBarRegistered = true;
                        lastPosition = transform.position;
                        lastHealthPercent = CurrentHealthPercent;
                        lastDamagePercent = healthBar.GetDamagePercent();
                    }
                }
                else
                {
                    // Health bar'ı güncelle (pozisyon veya health değiştiyse)
                    Vector3 currentPos = transform.position;
                    float currentHealthPercent = CurrentHealthPercent;
                    float currentDamagePercent = healthBar.GetDamagePercent();
                    
                    // Sadece değişiklik varsa güncelle
                    if (Vector3.Distance(lastPosition, currentPos) > 0.01f ||
                        Mathf.Abs(lastHealthPercent - currentHealthPercent) > 0.001f ||
                        Mathf.Abs(lastDamagePercent - currentDamagePercent) > 0.001f)
                    {
                        healthBarService.UpdateHealthBar(
                            gameObject,
                            currentPos,
                            currentHealthPercent,
                            currentDamagePercent
                        );
                        
                        lastPosition = currentPos;
                        lastHealthPercent = currentHealthPercent;
                        lastDamagePercent = currentDamagePercent;
                    }
                }
            }
            else if (hasHealthBarRegistered)
            {
                // Health bar'ı kaldır
                UnregisterHealthBar();
            }
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
                
            animator.CrossFade(IEnemyState.AnimNames.GetHit, 0f);
            Flash();
            
            _currentHealth -= amount;
            healthBar.TakeDamage(amount);
            
            // Health bar'ı göstermeye başla
            shouldShowHealthBar = true;
            _popupManager.ShowDamage(transform.position+Vector3.up*1.5f,amount.ToString(),Color.white);
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        
        public void Flash()
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
            healthBar.currentHealth = _currentHealth;
            
            // Health bar durumunu reset et
            shouldShowHealthBar = false;
            UnregisterHealthBar();
            
            gameObject.SetActive(true);
            enemyStateController.TransitionToIdle();
            agent.enabled = true;
            
            // Health bar verilerini güncelle
            lastPosition = transform.position;
            lastHealthPercent = CurrentHealthPercent;
            lastDamagePercent = healthBar.GetDamagePercent();
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