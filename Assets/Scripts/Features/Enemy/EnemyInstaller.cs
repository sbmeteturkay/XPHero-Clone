using Zenject;

namespace Game.Feature.Enemy
{
    public class EnemyInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<Enemy>().FromComponentInHierarchy().AsSingle();
            Container.Bind<EnemyMovement>().FromComponentInHierarchy().AsSingle();
            Container.Bind<EnemyAttack>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<EnemyStateController>().FromComponentInHierarchy().AsSingle();
        }
    }
}