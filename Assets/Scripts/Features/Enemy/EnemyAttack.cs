using Game.Core.Interfaces;
using Game.Core.Services;
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

        void Update()
        {
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }
        }

        public void StartAttack()
        {
            if (_attackCooldownTimer <= 0)
            {
                // Saldırı animasyonunu tetikle
                // GetComponent<Animator>().SetTrigger("Attack");

                // Hasar uygulama mantığı (animasyonun belirli bir frame'inde çağrılabilir)
                // Şimdilik doğrudan çağırıyoruz
                PerformAttack();

                _attackCooldownTimer = _enemy.Data.AttackCooldown;
            }
        }

        public void StopAttack()
        {
            // Saldırı animasyonunu durdur veya resetle
        }

        private void PerformAttack()
        {
            // Oyuncuyu hedef al
            IDamageable target = _playerService.PlayerTransform.GetComponent<IDamageable>();
            if (target != null)
            {
                _damageService.ApplyDamage(target, _enemy.Data.AttackDamage, _enemy.gameObject);
                Debug.Log($"{_enemy.Data.EnemyName} oyuncuya { _enemy.Data.AttackDamage} hasar verdi.");
            }
        }
    }
}