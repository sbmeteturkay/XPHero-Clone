using Zenject;

namespace Game.Feature.Player
{
    public class PlayerInstaller : MonoInstaller
    {
        [Inject] private SignalBus _signalBus;
        public override void InstallBindings()
        {
            Container.Bind<PlayerHealth>().FromComponentInHierarchy().AsSingle();
        }
    }
}