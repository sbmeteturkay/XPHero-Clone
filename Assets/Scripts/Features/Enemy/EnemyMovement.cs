using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyMovement : MonoBehaviour
    {
        [Inject] private Enemy _enemy;
        [SerializeField] private NavMeshAgent _navMeshAgent;

        void Awake()
        {
        }

        public void SetDestination(Vector3 destination)
        {
             _navMeshAgent.speed = _enemy.Data.MoveSpeed;
             _navMeshAgent.SetDestination(destination);
        }
    }
}