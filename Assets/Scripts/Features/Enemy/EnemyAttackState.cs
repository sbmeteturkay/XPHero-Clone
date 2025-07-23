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
            Debug.Log("Düşman: Attack Durumu");
            _enemyAttack.StartAttack();
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
            Debug.Log("Düşman: Attack Durumundan Çıkıldı");
            _enemyAttack.StopAttack();
        }
    }
}