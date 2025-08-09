using Game.Core.Services;
using UnityEngine;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyPatrolState : BaseState
    {
        private EnemyMovement _enemyMovement;
        
        private Vector3[] _patrolPoints;
        private int _currentPatrolIndex = 0;
        private float _patrolWaitTime = 2f; // Her patrol noktasında bekleme süresi
        private float _waitTimer = 0f;

        public EnemyPatrolState(EnemyStateController controller, Enemy enemy, PlayerService playerService) : base(controller, enemy, playerService)
        {
            _enemyMovement = enemy.EnemyMovement;
        }

        public override void Enter()
        {
            // Patrol noktalarını belirle (örneğin, spawn noktası etrafında rastgele noktalar)
            SetupPatrolPoints();

            // İlk patrol noktasına git
            MoveToNextPatrolPoint();
        }

        public override void Execute()
        {
            if (_controller.CanChase)
            {
                // Oyuncuyu menzil içinde mi kontrol et
                float distanceToPlayer = Vector3.Distance(_enemy.transform.position, _playerService.GetPlayerPosition());
                if (distanceToPlayer <
                    _enemy.Data.AttackRange * 2) // Saldırı menzilinin 2 katı kadar mesafede oyuncuyu fark et
                {
                    _controller.TransitionToChase();
                    return;
                }
            }

            // Mevcut patrol noktasına ulaştı mı kontrol et
            if (Vector3.Distance(_enemy.transform.position, _patrolPoints[_currentPatrolIndex]) < 1f)
            {
                // Patrol noktasında bekle
                _enemy.animator.CrossFade(IEnemyState.AnimNames.Idle, 0.2f);
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= _patrolWaitTime*Random.Range(1,5))
                {
                    _waitTimer = 0f;
                    MoveToNextPatrolPoint();
                }
            }
        }

        public override void Exit()
        {
        }

        //TODO: Patrol inside of given area
        private void SetupPatrolPoints()
        {
            // Spawn noktası etrafında rastgele patrol noktaları oluştur
            _patrolPoints = new Vector3[3]; // 3 patrol noktası

            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                _patrolPoints[i] = _enemy.GetRandomPosition();
            }
        }

        private void MoveToNextPatrolPoint()
        {
            _enemy.animator.CrossFade(IEnemyState.AnimNames.Walk, 0.2f);
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
            _enemyMovement.SetDestination(_patrolPoints[_currentPatrolIndex]);
        }
    }
}