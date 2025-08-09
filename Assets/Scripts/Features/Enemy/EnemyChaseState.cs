using Game.Core.Services;
using UnityEngine;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyChaseState : BaseState
    {
        private EnemyMovement _enemyMovement;

        public EnemyChaseState(EnemyStateController controller, Enemy enemy, PlayerService playerService) : base(controller, enemy, playerService)
        {
            _enemyMovement = enemy.EnemyMovement;
        }

        public override void Enter()
        {
            //animator.CrossFade(IEnemyState.AnimNames.Chase, 0.2f);
        }

        public override void Execute()
        {
            _enemyMovement.SetDestination(_playerService.PlayerTransform.position);

            if (Vector3.Distance(_enemy.transform.position, _playerService.PlayerTransform.position) < _enemy.Data.AttackRange)
            {
                _controller.TransitionToAttack();
            }
            else if (Vector3.Distance(_enemy.transform.position, _playerService.PlayerTransform.position) > 4)
            {
                //TODO: Turn back spawn area
                _controller.TransitionToIdle();
            }
        }

        public override void Exit()
        {
            _enemyMovement.StopMoving();
        }
    }
}