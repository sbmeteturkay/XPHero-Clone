using Zenject;

namespace Game.Core.Installer
{
    public class UISceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<UpgradeManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}