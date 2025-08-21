using UnityEngine;

namespace Game.Feature.Enemy
{
    public class EnemyDeactivatedState : BaseState
    {
        private float _waitTime;
        private float _timer;
        public EnemyDeactivatedState(EnemyStateController controller, Enemy enemy) : base(controller, enemy)
        {
            _waitTime = enemy.Data.respawnTime;
        }

        public override void Enter()
        {
            _enemy.isInteractingWithPlayer = false;
            _timer = 0;
            if(_enemy.gameObject.activeInHierarchy)
                _enemy.animator.Play(IEnemyState.AnimNames.Die);
        }

        public override void Execute()
        {
            _timer += Time.deltaTime;
            if (_timer >= 2)
            {
                if (_enemy.gameObject.activeSelf)
                {
                    _enemy.gameObject.SetActive(false);
                }
            }
            if (_timer >= _waitTime)
            {
                _enemy.CanSpawn.Value = true;
            }
        }
    }
}