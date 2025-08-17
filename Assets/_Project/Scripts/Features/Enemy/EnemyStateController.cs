// Scripts/Feature/Enemy/EnemyStateController.cs
using Zenject;
using Game.Core.Services;
using UnityEngine;

namespace Game.Feature.Enemy
{
    public class EnemyStateController
    {
        private readonly Enemy _enemy;
        private readonly PlayerService _playerService;

        private IEnemyState _currentState;

        // Durumlar
        private EnemyIdleState _idleState;
        private EnemyPatrolState _patrolState;
        private EnemyChaseState _chaseState;
        private EnemyAttackState _attackState;
        private EnemyDeactivatedState _deactivatedState;

        public EnemyStateController(Enemy enemy)
        {
            _enemy = enemy;
            _playerService = enemy.playerService;
        }

        public bool CanChase => _enemy.CanChaseOrAttack;
        public string CurrentState => _currentState.ToString();
        public void Initialize()
        {
            // Durumları oluştur ve inject et
            _idleState = new(this,_enemy);
            _patrolState = new(this,_enemy, _playerService);
            _chaseState = new(this,_enemy, _playerService);
            _attackState = new(this,_enemy, _playerService);
            _deactivatedState = new(this,_enemy);
            ChangeState(_deactivatedState);
        }

        //TODO: custom tick should be added
        public void Tick()
        {
            _currentState?.Execute();
        }

        public void ChangeState(IEnemyState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        // Diğer durum geçiş metodları
        public void TransitionToChase()
        {
            if (CanChase)
            {
                ChangeState(_chaseState);
            }
        }

        public void TransitionToAttack()
        {
            ChangeState(_attackState);
        }

        public void TransitionToIdle()
        {
            ChangeState(_idleState);
        }

        public void TransitionToPatrol()
        {
            ChangeState(_patrolState);
        }
        public void TransitionToDeactivate()
        {
            ChangeState(_deactivatedState);
        }

    }


    public interface IEnemyState
    {
        void Enter();
        void Execute();
        void Exit();
        public static class AnimNames
        {
            public static readonly string Idle = "Idle";
            public static readonly string Walk = "Walk";
            public static readonly string Attack = "Attack";
            public static readonly string Chase = "Chase";
            public static readonly string Die = "Die";
            public static readonly string GetHit = "GetHit";
        }
    }
}