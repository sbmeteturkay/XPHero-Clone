// Scripts/Core/Services/ProjectInputInstaller.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Core.Services
{
    [CreateAssetMenu(fileName = "ProjectInputInstaller", menuName = "Installers/ProjectInputInstaller")]
    public class ProjectInputInstaller : ScriptableObjectInstaller<ProjectInputInstaller>
    {
        [SerializeField] private InputActionAsset _inputActionAsset;

        public override void InstallBindings()
        {
            Container.Bind<InputActionAsset>()
                .FromInstance(_inputActionAsset)
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<InputService>()
                .AsSingle()
                .NonLazy();
        }
    }
}