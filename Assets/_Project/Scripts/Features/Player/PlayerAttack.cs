using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Game.Core.Services;
using Game.Feature.Spawn;
using PrimeTween;
using R3;

namespace Game.Feature.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [Inject] private DamageService _damageService;
        [Inject] private SignalBus _signalBus;
        [Inject] private PlayerService _playerService;
        [Inject] private SpawnManager spawnManager;
        [Inject] PlayerAnimationController playerAnimationController;
        private List<Enemy.Enemy> _targetableEnemies => spawnManager.GetTargetableEnemies();

        [SerializeField] private float _attackDamage = 1f; // Oyuncunun saldırı hasarı
        [SerializeField] private float _attackRange = 2f; // Oyuncunun saldırı menzili

        private bool _isAttacking;
        
        public readonly ReactiveProperty<Enemy.Enemy> HasEnemyInRange = new();

        private void OnEnable()
        {
            HasEnemyInRange.Subscribe(OnTargetEnemy);
        }
        private void OnTargetEnemy(Enemy.Enemy enemy)
        {
            playerAnimationController.SetBool("Attack", enemy);
            if (!enemy)
            {
                return;
            }
        }
        void Update()
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
            Vector3 direction = closestEnemy.transform.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime * 60f // derece/saniye
                );
            }
        }
        //animation event
        private void DealDamageEvent()
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
                _damageService.ApplyDamage(target, _attackDamage, gameObject);
            }
            //Debug.Log($"Oyuncu {targetEnemy.name} hedefine { _attackDamage} hasar verdi.");
        }

        private List<Enemy.Enemy> GetObjectsInCone( List<Enemy.Enemy> allObjects, float angleRange = 60f, float maxDistance = 10f)
        {
            List<Enemy.Enemy> result = new ();

            float cosThreshold = Mathf.Cos((angleRange * 0.5f) * Mathf.Deg2Rad); // ±30°

            foreach (var obj in allObjects)
            {
                
                Vector3 toObj = obj.transform.position - transform.position;
                float dist = toObj.magnitude;

                if (dist > maxDistance) continue; // range dışında

                Vector3 dirToObj = toObj.normalized;
                float dot = Vector3.Dot(transform.forward, dirToObj);

                if (dot >= cosThreshold) // ±30°
                {
                    result.Add(obj);
                }
            }

            return result;
        }
    }
}