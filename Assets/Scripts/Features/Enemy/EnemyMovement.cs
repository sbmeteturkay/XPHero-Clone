using UnityEngine;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyMovement : MonoBehaviour
    {
        [Inject] private Enemy _enemy;

        void Awake()
        {
        }

        public void SetDestination(Vector3 destination)
        {
            
             //_navMeshAgent.speed = _enemy.Data.MoveSpeed;
             //_navMeshAgent.SetDestination(destination);
        }

        // Diğer hareket metodları (örneğin, durma, rastgele hareket)
    }
}