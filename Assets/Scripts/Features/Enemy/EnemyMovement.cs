using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyMovement : MonoBehaviour
    {
        [Inject] private Enemy _enemy;
        [SerializeField] private NavMeshAgent _navMeshAgent;
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