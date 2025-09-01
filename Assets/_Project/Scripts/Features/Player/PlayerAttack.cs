using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Core.Services;
using Game.Feature.Spawn;
using UniRx;

namespace Game.Feature.Player
{
    public class PlayerAttack : IInitializable, IDisposable, ITickable
    {
        [Inject] private DamageService _damageService;
        [Inject] private SignalBus _signalBus;
        [Inject] private PlayerService _playerService;
        [Inject] private SpawnManager spawnManager;
        [Inject] PlayerAnimationController playerAnimationController;
        private List<Enemy.Enemy> _targetableEnemies => spawnManager.GetTargetableEnemies();

        private float _attackDamage = 1f; // Oyuncunun saldırı hasarı
        private float _attackSpeed = 1f; // Oyuncunun saldırı hızı
        private readonly float _attackRange = 2f; // Oyuncunun saldırı menzili

        private bool _isAttacking;
        
        public readonly ReactiveProperty<Enemy.Enemy> HasEnemyInRange = new();
        private readonly CompositeDisposable _disposables=new();
        public void Initialize()
        {
            HasEnemyInRange.Subscribe(OnTargetEnemy).AddTo(_disposables);
            _playerService.PlayerUpgradeData.Subscribe(OnUpgradeDataChanged).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnUpgradeDataChanged(UpgradeData upgradeData)
        {
            _attackDamage = upgradeData.Power;
            _attackSpeed = upgradeData.AttackSpeed*0.01f;
            playerAnimationController.SetFloat("AttackSpeed", _attackSpeed);
        }

        private void OnTargetEnemy(Enemy.Enemy enemy)
        {
            playerAnimationController.SetBool("Attack", enemy);
        }
        public void Tick()
        {
            if (_targetableEnemies.Count <= 0)
            {
                HasEnemyInRange.Value = null;
                return;
            }
            
            Enemy.Enemy closestEnemy = null;
            float closestDistanceSqr = _attackRange * _attackRange;

            Vector3 playerPos = _playerService.GetPlayerPosition();

            foreach (var enemy in _targetableEnemies)
            {
                float distSqr = (enemy.transform.position - playerPos).sqrMagnitude;

                if (distSqr <= closestDistanceSqr)
                {
                    if (closestEnemy == null ||
                        distSqr < (closestEnemy.transform.position - playerPos).sqrMagnitude)
                    {
                        closestEnemy = enemy;
                        closestDistanceSqr = distSqr;
                    }
                }
            }

            HasEnemyInRange.Value = closestEnemy;
            if(closestEnemy==null)
                return;
            float rotationSpeed = 5f; // saniyedeki dönüş hızı
            Vector3 direction = closestEnemy.transform.position - _playerService.GetPlayerPosition();
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _playerService.PlayerTransform.rotation = Quaternion.RotateTowards(
                    _playerService.PlayerTransform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime * 60f // derece/saniye
                );
            }
        }
        //animation event
        public void DealDamageEvent()
        {
            if (HasEnemyInRange.Value)
            {
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            var targetsInRange = GetObjectsInCone(_targetableEnemies, 120, _attackRange);
            foreach (var target in targetsInRange)
            {
                _damageService.ApplyDamage(target, _attackDamage, _playerService.PlayerTransform.gameObject);
            }
            //Debug.Log($"Oyuncu {targetEnemy.name} hedefine { _attackDamage} hasar verdi.");
        }

        private List<Enemy.Enemy> GetObjectsInCone( List<Enemy.Enemy> allObjects, float angleRange = 60f, float maxDistance = 10f)
        {
            List<Enemy.Enemy> result = new ();

            float cosThreshold = Mathf.Cos((angleRange * 0.5f) * Mathf.Deg2Rad); // ±30°

            foreach (var obj in allObjects)
            {
                
                Vector3 toObj = obj.transform.position - _playerService.GetPlayerPosition();
                float dist = toObj.magnitude;

                if (dist > maxDistance) continue; // range dışında

                Vector3 dirToObj = toObj.normalized;
                float dot = Vector3.Dot(_playerService.PlayerTransform.forward, dirToObj);

                if (dot >= cosThreshold) // ±30°
                {
                    result.Add(obj);
                }
            }

            return result;
        }
    }
}