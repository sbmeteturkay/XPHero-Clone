using Game.Core.Services;
using UnityEngine;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyAttackState : BaseState
    {

        private float _attackCooldownTimer;
        public bool CanAttack { get; set; }
        public EnemyAttackState(EnemyStateController controller, Enemy enemy, PlayerService playerService) : base(controller, enemy)
        {
            _enemy = enemy;
            _playerService = playerService;
            _attackCooldownTimer = _enemy.Data.AttackCooldown;
        }

        public override void Enter()
        {
            CanAttack = true;
        }

        public override void Execute()
        {
            RotateToTarget(_playerService.PlayerTransform);
            if (Vector3.Distance(_enemy.transform.position, _playerService.PlayerTransform.position) > _enemy.Data.AttackRange)
            {
                _controller.TransitionToChase();
                return;
            }
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }
            else if( CanAttack )
            {
                PerformAttack();
            }
        }

        public override void Exit()
        {
            CanAttack = false;
        }
        private void PerformAttack()
        {
            _enemy.animator.CrossFade(IEnemyState.AnimNames.Attack, 0f);
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
                    rotationSpeed * Time.deltaTime * 60f // derece/saniye
                );
            }
        }
    }
}