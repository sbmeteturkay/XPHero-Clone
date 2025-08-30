using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Core.Services
{
    [CreateAssetMenu(fileName = "ProjectDataInstaller", menuName = "Installers/ProjectDataInstaller")]
    public class ProjectDataInstaller : ScriptableObjectInstaller<ProjectDataInstaller>
    {
        public override void InstallBindings()
        {
            var initialData = new UpgradeData(); // default değer
            Container.Bind<ReactiveProperty<UpgradeData>>()
                .FromInstance(new ReactiveProperty<UpgradeData>(initialData))
                .AsSingle();
        }
    }
}