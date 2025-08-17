using System.Collections;
using Game.Core.Interfaces;
using Game.Core.Services;
using PrimeTween;
using UnityEngine;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyAttack : ITickable
    {
        [Inject] private PlayerService _playerService;
        private Enemy _enemy;

        private float _attackCooldownTimer;
        public bool CanAttack { get; set; }

        public EnemyAttack(Enemy enemy)
        {
            _enemy = enemy;
        }
        public void Tick()
        {
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }
            else if( CanAttack )
            {
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            RotateToTarget(_playerService.PlayerTransform);
            _attackCooldownTimer = _enemy.Data.AttackCooldown;
            //_damageService.ApplyDamage(_playerService.PlayerDamageable, _enemy.Data.AttackDamage, _enemy.gameObject);
            //Debug.Log($"{_enemy.Data.EnemyName} oyuncuya { _enemy.Data.AttackDamage} hasar verdi.");
        }
         void RotateToTarget(Transform target)
        {
            float rotationSpeed = 20f; // saniyedeki dönüş hızı
            Vector3 direction = target.transform.position - _enemy.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _enemy.transform.rotation = Quaternion.RotateTowards(
                    _enemy.transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime * 360f // derece/saniye
                );
            }
        }
    }
}