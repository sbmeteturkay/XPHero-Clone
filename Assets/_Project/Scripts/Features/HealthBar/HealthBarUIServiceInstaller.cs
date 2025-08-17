using UnityEngine;
using Zenject;

namespace Game.Feature.UI
{
    public class HealthBarUIServiceInstaller : MonoInstaller
    {   
        [Header("Mesh & Materials")]
        [SerializeField] Mesh quadMesh;
        [SerializeField] Material backMaterial; // white background (instancing enabled)
        [SerializeField] Material fillMaterial; // red fill (instancing enabled)
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HealthBarUIService>().AsSingle().WithArguments(quadMesh, backMaterial,fillMaterial);
        }
    }
}