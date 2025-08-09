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
        
        public readonly ReactiveProperty<Enemy.Enemy> HasTargetEnemy = new();

        private void OnEnable()
        {
            HasTargetEnemy.Subscribe(OnTargetEnemy);
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
                HasTargetEnemy.Value = null;
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

            HasTargetEnemy.Value = closestEnemy;
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
                    rotationSpeed * Time.deltaTime * 360f // derece/saniye
                );
            }
        }
        //animation event
        private void DealDamageEvent()
        {
            if (HasTargetEnemy.Value)
            {
                PerformAttack(HasTargetEnemy.Value);
            }
        }

        private void PerformAttack(Game.Feature.Enemy.Enemy targetEnemy)
        {
            if (Vector3.Distance(_playerService.GetPlayerPosition(), targetEnemy.transform.position) <= _attackRange)
            {
                _damageService.ApplyDamage(targetEnemy, _attackDamage, gameObject);
            }
            //Debug.Log($"Oyuncu {targetEnemy.name} hedefine { _attackDamage} hasar verdi.");
        }
    }
}