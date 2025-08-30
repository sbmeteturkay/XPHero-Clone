using UnityEngine;
using Zenject;
// Zenject Installer
public class HealthBarInstaller : MonoInstaller
{
    [SerializeField] private Material healthBarMaterial;
    [SerializeField] private HealthBarSettings settings;

    public override void InstallBindings()
    {
        Container.Bind<HealthBarSettings>().FromInstance(settings).AsSingle();
        Container.Bind<IMaterialProvider>().To<MaterialProvider>().AsSingle().WithArguments(healthBarMaterial);
        Container.Bind<IMeshProvider>().To<MeshProvider>().AsSingle();
        Container.Bind<ICameraProvider>().To<CameraProvider>().AsSingle();
        Container.Bind<IHealthBarRenderer>().To<HealthBarRenderer>().AsSingle();
        Container.BindInterfacesAndSelfTo<HealthBarRegistry>().AsSingle();
        Container.BindInterfacesAndSelfTo<HealthBarManager>().AsSingle();
    }

    private class MaterialProvider : IMaterialProvider
    {
        private readonly Material _material;

        public MaterialProvider(Material material)
        {
            _material = material;
        }

        public Material GetHealthBarMaterial() => _material;
    }
}