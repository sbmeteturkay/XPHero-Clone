using System.Collections;
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
            StopAllCoroutines();
            StartCoroutine(RotateToTarget(_playerService.PlayerTransform));
            _attackCooldownTimer = _enemy.Data.AttackCooldown;
            //_damageService.ApplyDamage(_playerService.PlayerDamageable, _enemy.Data.AttackDamage, _enemy.gameObject);
            //Debug.Log($"{_enemy.Data.EnemyName} oyuncuya { _enemy.Data.AttackDamage} hasar verdi.");
        }
        IEnumerator RotateToTarget(Transform target)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
                yield break;

            Quaternion start = transform.rotation;
            Quaternion end = Quaternion.LookRotation(direction);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.2f;
                transform.rotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }
        }
    }
}