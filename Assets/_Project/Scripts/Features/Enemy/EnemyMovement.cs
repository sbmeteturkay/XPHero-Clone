using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyMovement
    {
        private Enemy _enemy;
        private NavMeshAgent _navMeshAgent;

        public EnemyMovement(NavMeshAgent navMeshAgent,Enemy enemy)
        {
            _navMeshAgent = navMeshAgent;
            _enemy = enemy;
        }
        public void SetDestination(Vector3 destination)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.speed = _enemy.Data.MoveSpeed;
            _navMeshAgent.SetDestination(destination);
        }

        public void StopMoving()
        {
            _navMeshAgent.isStopped = true;
        }
    }
}