using System;
using Zenject;

namespace Game.Feature.Player
{
    public class PlayerInstaller : MonoInstaller
    {
        [Inject] private SignalBus _signalBus;
        public override void InstallBindings()
        {
            Container.Bind<PlayerAnimationController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PlayerAttack>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle().NonLazy();
        }
    }
}