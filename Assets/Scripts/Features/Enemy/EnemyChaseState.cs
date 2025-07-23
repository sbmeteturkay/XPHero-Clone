using Game.Core.Services;
using UnityEngine;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyChaseState : IEnemyState
    {
        private EnemyStateController _controller;
        private Enemy _enemy;
        private PlayerService _playerService;
        [Inject] private EnemyMovement _enemyMovement; // Zenject ile inject edilebilir

        public void Enter(EnemyStateController controller, Enemy enemy, PlayerService playerService)
        {
            _controller = controller;
            _enemy = enemy;
            _playerService = playerService;
            enemy.animator.CrossFade(IEnemyState.AnimNames.Chase, 0.2f);
            Debug.Log("Düşman: Chase Durumu");
        }

        public void Execute()
        {
            _enemyMovement.SetDestination(_playerService.PlayerTransform.position);

            if (Vector3.Distance(_enemy.transform.position, _playerService.PlayerTransform.position) < _enemy.Data.AttackRange)
            {
                _controller.TransitionToAttack();
            }
            else if (Vector3.Distance(_enemy.transform.position, _playerService.PlayerTransform.position) > 4)
            {
                _controller.TransitionToIdle();
            }
        }

        public void Exit()
        {
            _enemyMovement.StopMoving();
        }
    }
}