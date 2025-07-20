using Game.Core.Services;
using Zenject;

namespace Game.Core.Installer
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [Inject] InputService inputService;
        public override void InstallBindings()
        {
            inputService.EnablePlayerInput();
        }
        void OnDisable()
        {
            inputService.DisablePlayerInput();
        }
    }
}