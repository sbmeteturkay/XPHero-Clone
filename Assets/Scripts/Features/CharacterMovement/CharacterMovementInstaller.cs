// Scripts/Feature/CharacterMovement/CharacterMovementInstaller.cs

using UnityEngine;
using Zenject;

namespace Game.Feature.CharacterMovement
{
    public class CharacterMovementInstaller : MonoInstaller
    {
        [SerializeField] GameObject character;
        public override void InstallBindings()
        {
            // Model, View, Controller/Presenter/ViewModel bindingleri burada yapılır
            Container.BindInterfacesAndSelfTo<CharacterMovementModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<CharacterMovementController>().AsSingle();
            Container.Bind<CharacterMovementView>().FromComponentOn(character).AsSingle();
            // EventBus'a abone olma gibi feature özel servisler
            // Container.Bind<CharacterMovementService>().AsSingle();
        }

        public override void Start()
        {
            base.Start();
        }
    }
}