// Scripts/Core/Services/PlayerTargetingService.cs (Güncelleme)

using System;
using System.Linq;
using UnityEngine;
using Zenject;
using UniRx;
using Game.Feature.Spawn;

namespace Game.Core.Services
{
    public class PlayerTargetingService : IInitializable, ITickable, IDisposable
    {
        [Inject] private PlayerService _playerService;
        [Inject] private SignalBus _signalBus;

        private float _targetingCheckInterval = 0.1f; // Hedefleme kontrol sıklığı (saniye)
        private float _targetingTimer;

        private Feature.Enemy.Enemy _currentTarget; // Oyuncunun mevcut hedefi
        private SpawnArea _currentActiveSpawnArea; // Oyuncunun içinde bulunduğu aktif spawn alanı

        public Game.Feature.Enemy.Enemy CurrentTarget => _currentTarget;

        public void Initialize()
        {
            // Düşman spawn ve despawn sinyallerine abone ol (hedef listesini güncel tutmak için)
            _signalBus.Subscribe<Game.Feature.Enemy.EnemyDiedSignal>(OnEnemyDied);

            // Oyuncunun spawn alanına giriş/çıkış sinyallerine abone ol
            _signalBus.Subscribe<PlayerEnteredSpawnAreaSignal>(OnPlayerEnteredSpawnArea);
            _signalBus.Subscribe<PlayerExitedSpawnAreaSignal>(OnPlayerExitedSpawnArea);
        }

        public void Tick()
        {
            _targetingTimer -= Time.deltaTime;
            if (_targetingTimer <= 0)
            {
                PerformTargetingCheck();
                _targetingTimer = _targetingCheckInterval;
            }
        }

        private void PerformTargetingCheck()
        {

        }

        private void OnEnemyDied(Game.Feature.Enemy.EnemyDiedSignal signal)
        {
            if (signal.Enemy == _currentTarget)
            {
                _currentTarget = null; // Ölen düşman hedeftiyse hedefi sıfırla
                _signalBus.Fire(new PlayerTargetLostSignal { OldTarget = signal.Enemy });
            }
            // PerformTargetingCheck(); // Hedef ölmüşse yeni hedef ara
        }

        private void OnPlayerEnteredSpawnArea(PlayerEnteredSpawnAreaSignal signal)
        {
            _currentActiveSpawnArea = signal.SpawnArea;
            Debug.Log($"PlayerTargetingService: Oyuncu {signal.SpawnArea.name} alanına girdi.");
            PerformTargetingCheck(); // Yeni alana girince hemen hedef kontrolü yap
        }

        private void OnPlayerExitedSpawnArea(PlayerExitedSpawnAreaSignal signal)
        {
            if (_currentActiveSpawnArea == signal.SpawnArea)
            {
                _currentActiveSpawnArea = null; // Oyuncu alandan çıktıysa aktif alanı sıfırla
                Debug.Log($"PlayerTargetingService: Oyuncu {signal.SpawnArea.name} alanından çıktı.");
            }
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<Feature.Enemy.EnemyDiedSignal>(OnEnemyDied);
            _signalBus.Unsubscribe<PlayerEnteredSpawnAreaSignal>(OnPlayerEnteredSpawnArea);
            _signalBus.Unsubscribe<PlayerExitedSpawnAreaSignal>(OnPlayerExitedSpawnArea);
        }
    }

    // Zenject Signal Tanımları (zaten tanımlıydı)
     public class PlayerTargetFoundSignal { public Game.Feature.Enemy.Enemy NewTarget; }
     public class PlayerTargetLostSignal { public Game.Feature.Enemy.Enemy OldTarget; }
}