using Game.Core.Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Feature.Enemy
{
    public class EnemyIdleState : BaseState
    {
        private float _waitTime;
        private float _timer;

        public EnemyIdleState(EnemyStateController controller, Enemy enemy, PlayerService playerService) : base(controller, enemy, playerService)
        {
        }

        public override void Enter()
        {
            _waitTime = Random.Range(0f, 3f);
            _timer = 0f;
            _enemy.animator.CrossFade(IEnemyState.AnimNames.Idle, 0.2f);
        }

        public override void Execute()
        {
            _timer += Time.deltaTime;
            if (_timer >= _waitTime)
            {
                _controller.TransitionToPatrol();
            }
        }

    }
    public class EnemyDeactivatedState : BaseState
    {
        private float _waitTime;
        private float _timer;
        public EnemyDeactivatedState(EnemyStateController controller, Enemy enemy, PlayerService playerService) : base(controller, enemy, playerService)
        {
            _waitTime = enemy.Data.respawnTime;
        }

        public override void Enter()
        {
            _timer = 0;
        }

        public override void Execute()
        {
            _timer += Time.deltaTime;
            if (_timer >= _waitTime)
            {
                _enemy.CanSpawn.Value = true;
            }
        }
    }
}