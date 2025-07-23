using System;
using Cysharp.Threading.Tasks;
using Game.Core.Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Feature.Enemy
{
    public class EnemyIdleState : IEnemyState
    {
        private static readonly int Idle = Animator.StringToHash("Idle");
        private EnemyStateController _controller;
        private Enemy _enemy;
        private PlayerService _playerService;

        public void Enter(EnemyStateController controller, Enemy enemy, PlayerService playerService)
        {
            _controller = controller;
            _enemy = enemy;
            _playerService = playerService;
            Debug.Log("Düşman: Idle Durumu");
            DelayThenDoSomething();
            _enemy.animator.CrossFade(IEnemyState.AnimNames.Idle, 0f);
        }
        private async void DelayThenDoSomething()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(0f, 3f)));
            _controller.TransitionToPatrol();
        }
        public void Execute()
        {
            // Oyuncuyu menzil içinde mi kontrol et
            Debug.Log(_enemy.transform.position);
            Debug.Log(_playerService.GetPlayerPosition());
            Debug.Log(_enemy.Data.AttackRange);
            // if (Vector3.Distance(_enemy.transform.position, _playerService.GetPlayerPosition()) < _enemy.Data.AttackRange * 2)
            // {
            //     //_controller.TransitionToChase();
            // }
        }

        public void Exit()
        {
            Debug.Log("Düşman: Idle Durumundan Çıkıldı");
        }
    }
}