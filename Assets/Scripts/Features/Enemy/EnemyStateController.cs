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
        public void TransitionToChase() { ChangeState(_chaseState); }
        public void TransitionToAttack() { ChangeState(_attackState); }
        public void TransitionToIdle() { ChangeState(_idleState); }
        public void TransitionToPatrol() { ChangeState(_patrolState); }

    }


    public interface IEnemyState
    {
        void Enter(EnemyStateController controller, Enemy enemy, PlayerService playerService);
        void Execute();
        void Exit();
    }

    // Örnek Durum Sınıfları
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
            Debug.Log("Düşman: Idle Durumu");
        }

        public void Execute()
        {
            // Oyuncuyu menzil içinde mi kontrol et
            if (Vector3.Distance(_enemy.transform.position, _playerService.PlayerTransform.position) < _enemy.Data.AttackRange * 2)
            {
                //_controller.TransitionToChase();
            }
        }

        public void Exit()
        {
            Debug.Log("Düşman: Idle Durumundan Çıkıldı");
        }
    }

    public class EnemyChaseState : IEnemyState
    {
        private EnemyStateController _controller;
        private Enemy _enemy;
        private PlayerService _playerService;
        [Inject] private EnemyMovement _enemyMovement; // Zenject ile inject edilebilir

        public void Enter(EnemyStateController controller, Enemy enemy, PlayerService playerService)
        {
            _controller = controller;
            _enemy = enemy;
            _playerService = playerService;
            Debug.Log("Düşman: Chase Durumu");
        }

        public void Execute()
        {
            _enemyMovement.SetDestination(_playerService.PlayerTransform.position);

            if (Vector3.Distance(_enemy.transform.position, _playerService.PlayerTransform.position) < _enemy.Data.AttackRange)
            {
                _controller.TransitionToAttack();
            }
        }

        public void Exit()
        {
            Debug.Log("Düşman: Chase Durumundan Çıkıldı");
        }
    }

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
    public class EnemyPatrolState : IEnemyState
    {
        [Inject] private EnemyMovement _enemyMovement;

        private EnemyStateController _controller;
        private Enemy _enemy;
        private PlayerService _playerService;

        private Vector3[] _patrolPoints;
        private int _currentPatrolIndex = 0;
        private float _patrolWaitTime = 2f; // Her patrol noktasında bekleme süresi
        private float _waitTimer = 0f;

        public void Enter(EnemyStateController controller, Enemy enemy, PlayerService playerService)
        {
            _controller = controller;
            _enemy = enemy;
            _playerService = playerService;

            Debug.Log($"{_enemy.Data.EnemyName}: Patrol Durumu");

            // Patrol noktalarını belirle (örneğin, spawn noktası etrafında rastgele noktalar)
            SetupPatrolPoints();
            
            // İlk patrol noktasına git
            MoveToNextPatrolPoint();
        }

        public void Execute()
        {
            // Oyuncuyu menzil içinde mi kontrol et
            float distanceToPlayer = Vector3.Distance(_enemy.transform.position, _playerService.GetPlayerPosition());
            if (distanceToPlayer < _enemy.Data.AttackRange * 2) // Saldırı menzilinin 2 katı kadar mesafede oyuncuyu fark et
            {
                _controller.TransitionToChase();
                return;
            }

            // Mevcut patrol noktasına ulaştı mı kontrol et
            if (Vector3.Distance(_enemy.transform.position, _patrolPoints[_currentPatrolIndex]) < 1f)
            {
                // Patrol noktasında bekle
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= _patrolWaitTime)
                {
                    _waitTimer = 0f;
                    MoveToNextPatrolPoint();
                }
            }
        }

        public void Exit()
        {
            Debug.Log($"{_enemy.Data.EnemyName}: Patrol Durumundan Çıkıldı");
        }

        private void SetupPatrolPoints()
        {
            // Spawn noktası etrafında rastgele patrol noktaları oluştur
            Vector3 spawnPosition = _enemy.transform.position;
            _patrolPoints = new Vector3[3]; // 3 patrol noktası

            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 10f; // 10 birim yarıçapında
                randomDirection.y = 0; // Y eksenini sıfırla (yatay hareket)
                _patrolPoints[i] = spawnPosition + randomDirection;
            }
        }

        private void MoveToNextPatrolPoint()
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
            _enemyMovement.SetDestination(_patrolPoints[_currentPatrolIndex]);
            Debug.Log($"{_enemy.Data.EnemyName}: Patrol noktası {_currentPatrolIndex}'e gidiyor.");
        }
    }
}