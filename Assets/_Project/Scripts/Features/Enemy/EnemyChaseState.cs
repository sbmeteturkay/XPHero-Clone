using Game.Core.Services;
using UnityEngine;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyChaseState : BaseState
    {
        private EnemyMovement _enemyMovement;

        public EnemyChaseState(EnemyStateController controller, Enemy enemy, PlayerService playerService) : base(controller, enemy)
        {
            _enemy = enemy;
            _enemyMovement = enemy.EnemyMovement;
            _playerService = playerService;
        }

        public override void Enter()
        {
            //animator.CrossFade(IEnemyState.AnimNames.Chase, 0.2f);
            _enemy.isInteractingWithPlayer = true;
        }

        public override void Execute()
        {
            _enemyMovement.SetDestination(_playerService.GetPlayerPosition());
            
            if (Vector3.Distance(_enemy.transform.position, _playerService.GetPlayerPosition()) < _enemy.Data.AttackRange)
            {
                _controller.TransitionToAttack();
            }
            else if (Vector3.Distance(_enemy.transform.position, _playerService.GetPlayerPosition()) > 4||_enemy.DistanceFromSpawnPoint() > 10)
            {
                _enemy.isInteractingWithPlayer = false;
                _controller.TransitionToPatrol();
            }
        }

        public override void Exit()
        {
            _enemyMovement.StopMoving();
        }
    }
}