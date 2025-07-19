using Game.Core.Services;
using Zenject;

namespace Game.Core.Installer
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [Inject] InputService inputService;

        void OnEnable()
        {
            inputService.EnablePlayerInput();
        }

        void OnDisable()
        {
            inputService.DisablePlayerInput();
        }
    }
}