using Game.Core.Interfaces;
using Game.Core.Services;
using PrimeTween;
using UnityEngine;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyAttack : MonoBehaviour
    {
        [Inject] private DamageService _damageService;
        [Inject] private PlayerService _playerService;
        [Inject] private Enemy _enemy;

        private float _attackCooldownTimer;
        public bool CanAttack { get; set; }

        void Update()
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
            _damageService.ApplyDamage(_playerService.PlayerDamageable, _enemy.Data.AttackDamage, _enemy.gameObject);
            //Debug.Log($"{_enemy.Data.EnemyName} oyuncuya { _enemy.Data.AttackDamage} hasar verdi.");
        }
        void RotateToTarget(Transform target)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Tween.Rotation(
                transform,
                targetRotation,
                duration: _enemy.Data.AttackCooldown/2,
                Ease.InOutSine
            );
        }
    }
}