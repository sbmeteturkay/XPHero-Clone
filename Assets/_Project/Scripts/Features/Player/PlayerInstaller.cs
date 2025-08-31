using UnityEngine;
using Zenject;

namespace Game.Feature.Player
{
    public class PlayerInstaller : MonoInstaller
    {
        [Inject] private SignalBus _signalBus;
        private PlayerAttack _playerAttack;
        public override void InstallBindings()
        {
            Container.Bind<PlayerAnimationController>().FromComponentInHierarchy().AsSingle(); 
            Container.BindInterfacesAndSelfTo<PlayerAttack>().AsSingle();
            Debug.Log("PlayerHealth service binded");
            Container.BindInterfacesAndSelfTo<PlayerHealth>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle().NonLazy();
        }

        public override void Start()
        {
            _playerAttack = Container.Resolve<PlayerAttack>();
        }

        private void DealDamageEvent()
        {
            _playerAttack.DealDamageEvent();
        }
    }
}