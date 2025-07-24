using System;
using Cysharp.Threading.Tasks;
using Game.Core.Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Feature.Enemy
{
    public class EnemyIdleState : IEnemyState
    {
        private EnemyStateController _controller;
        private Enemy _enemy;
        private PlayerService _playerService;

        public void Enter(EnemyStateController controller, Enemy enemy, PlayerService playerService)
        {
            _controller = controller;
            _enemy = enemy;
            _playerService = playerService;
            DelayThenDoSomething();
            _enemy.animator.CrossFade(IEnemyState.AnimNames.Idle, 0.2f);
        }
        private async void DelayThenDoSomething()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(0f, 3f)));
            _controller.TransitionToPatrol();
        }
        public void Execute()
        {
        }

        public void Exit()
        {
        }
    }
}