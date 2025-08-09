using Game.Core.Services;
using UnityEngine;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyAttackState : BaseState
    {

        private EnemyAttack _enemyAttack; // Zenject ile inject edilebilir

        public EnemyAttackState(EnemyStateController controller, Enemy enemy, PlayerService playerService) : base(controller, enemy, playerService)
        {
            _enemyAttack = enemy.EnemyAttack;
        }

        public override void Enter()
        {
            _enemy.animator.CrossFade(IEnemyState.AnimNames.Attack, 0.2f);
            _enemyAttack.CanAttack = true;
        }

        public override void Execute()
        {
            if (Vector3.Distance(_enemy.transform.position, _playerService.PlayerTransform.position) > _enemy.Data.AttackRange)
            {
                _controller.TransitionToChase();
            }
        }

        public override void Exit()
        {
            _enemyAttack.CanAttack = false;
        }
    }
}