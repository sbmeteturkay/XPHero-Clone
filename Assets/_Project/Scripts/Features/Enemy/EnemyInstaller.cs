using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyInstaller : MonoInstaller
    {
        [SerializeField] NavMeshAgent agent;
        public override void InstallBindings()
        {
            Container.Bind<Enemy>().FromComponentOnRoot().AsSingle();

            Container.Bind<EnemyMovement>()
                .AsTransient()
                .WithArguments(agent);

            Container.Bind<EnemyAttack>().AsTransient();
        }
    }
}