using Game.Core.Services;

namespace Game.Feature.Enemy
{
    public class BaseState : IEnemyState
    {
        protected EnemyStateController _controller;
        protected Enemy _enemy;
        protected PlayerService _playerService;

        protected BaseState(EnemyStateController controller, Enemy enemy, PlayerService playerService)
        {
            _controller = controller;
            _enemy = enemy;
            _playerService = playerService;
        }
        public virtual void Enter()
        {
        }

        public virtual void Execute()
        {
        }

        public virtual void Exit()
        {
        }
    }
}