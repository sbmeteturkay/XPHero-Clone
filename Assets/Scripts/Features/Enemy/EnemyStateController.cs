// Scripts/Feature/Enemy/EnemyStateController.cs
using UnityEngine;
using Zenject;
using Game.Core.Services;

namespace Game.Feature.Enemy
{
    public class EnemyStateController : MonoBehaviour, ITickable, IInitializable
    {
        [Inject] private Enemy _enemy;
        [Inject] private PlayerService _playerService; // Oyuncuyu bulmak için
        [Inject] private DiContainer _container; // Durumları inject etmek için

        private IEnemyState _currentState;

        // Durumlar
        private EnemyIdleState _idleState;
        private EnemyPatrolState _patrolState;
        private EnemyChaseState _chaseState;
        private EnemyAttackState _attackState;

        public bool CanChase => _enemy.CanChaseOrAttack;

        void Awake()
        {
            // Durumları oluştur ve inject et
            _idleState = _container.Instantiate<EnemyIdleState>();
            _patrolState = _container.Instantiate<EnemyPatrolState>();
            _chaseState = _container.Instantiate<EnemyChaseState>();
            _attackState = _container.Instantiate<EnemyAttackState>();
        }
        public void Initialize()
        {
            // Başlangıç durumu
            ChangeState(_idleState);
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
            _currentState.Enter(this, _enemy, _playerService);
        }

        // Diğer durum geçiş metodları
        public void TransitionToChase()
        {
            if (CanChase)
                ChangeState(_chaseState);
        }
        public void TransitionToAttack() { ChangeState(_attackState); }
        public void TransitionToIdle() { ChangeState(_idleState); }
        public void TransitionToPatrol() { ChangeState(_patrolState); }

    }


    public interface IEnemyState
    {
        void Enter(EnemyStateController controller, Enemy enemy, PlayerService playerService);
        void Execute();
        void Exit();
        public static class AnimNames
        {
            public static readonly string Idle = "Idle";
            public static readonly string Walk = "Walk";
            public static readonly string Attack = "Attack";
            public static readonly string Chase = "Chase";
        }
    }
}