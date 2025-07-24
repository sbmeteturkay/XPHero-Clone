// Scripts/Core/Services/EnemyDetectionService.cs (Güncelleme)
using UnityEngine;
using Zenject;
 using System.Linq;
using Game.Feature.Enemy;
using Game.Feature.Spawn;
namespace Game.Core.Services
{
    public class EnemyDetectionService : IInitializable, ITickable, System.IDisposable
    {
        [Inject] private SignalBus _signalBus;

        private SpawnArea _currentActiveSpawnArea; // Oyuncunun içinde bulunduğu aktif spawn alanı

        private float _detectionCheckInterval = 0.2f; // Algılama kontrol sıklığı (saniye)
        private float _detectionTimer;
        public void Initialize()
        {
            // Oyuncunun spawn alanına giriş/çıkış sinyallerine abone ol
            _signalBus.Subscribe<PlayerEnteredSpawnAreaSignal>(OnPlayerEnteredSpawnArea);
            _signalBus.Subscribe<PlayerExitedSpawnAreaSignal>(OnPlayerExitedSpawnArea);
        }

        public void Tick()
        {
            _detectionTimer -= Time.deltaTime;
            if (_detectionTimer <= 0)
            {
                PerformDetectionCheck();
                _detectionTimer = _detectionCheckInterval;
            }
        }

        private void PerformDetectionCheck()
        {
        }

        private void OnPlayerEnteredSpawnArea(PlayerEnteredSpawnAreaSignal signal)
        {
            _currentActiveSpawnArea = signal.SpawnArea;
            Debug.Log($"EnemyDetectionService: Oyuncu {signal.SpawnArea.name} alanına girdi.");
        }

        private void OnPlayerExitedSpawnArea(PlayerExitedSpawnAreaSignal signal)
        {
            if (_currentActiveSpawnArea == signal.SpawnArea)
            {
                _currentActiveSpawnArea = null; // Oyuncu alandan çıktıysa aktif alanı sıfırla
                Debug.Log($"EnemyDetectionService: Oyuncu {signal.SpawnArea.name} alanından çıktı.");
            }
        }

        public void Dispose()
        {
            // Oyuncunun spawn alanına giriş/çıkış sinyallerine abone ol
            _signalBus.Unsubscribe<PlayerEnteredSpawnAreaSignal>(OnPlayerEnteredSpawnArea);
            _signalBus.Unsubscribe<PlayerExitedSpawnAreaSignal>(OnPlayerExitedSpawnArea);
        }
    }
}