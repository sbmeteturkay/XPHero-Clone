using Game.Core.Services;
using UnityEngine;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyAttackState : IEnemyState
    {
        private EnemyStateController _controller;
        private Enemy _enemy;
        private PlayerService _playerService;
        [Inject] private EnemyAttack _enemyAttack; // Zenject ile inject edilebilir

        public void Enter(EnemyStateController controller, Enemy enemy, PlayerService playerService)
        {
            _controller = controller;
            _enemy = enemy;
            _playerService = playerService;
            _enemy.animator.CrossFade(IEnemyState.AnimNames.Attack, 0.2f);
            _enemyAttack.CanAttack = true;
        }

        public void Execute()
        {
            if (Vector3.Distance(_enemy.transform.position, _playerService.PlayerTransform.position) > _enemy.Data.AttackRange)
            {
                _controller.TransitionToChase();
            }
        }

        public void Exit()
        {
            _enemyAttack.CanAttack = false;
        }
        
    }
}