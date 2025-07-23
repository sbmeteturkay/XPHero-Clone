using UnityEngine;
using Zenject;
using System.Collections.Generic;
using System.Linq;
using Game.Feature.Enemy;

namespace Game.Feature.Spawn
{
    public class SpawnManager : IInitializable, ITickable, System.IDisposable
    {
        [Inject] private DiContainer _container;
        [Inject] private SignalBus _signalBus;

        private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        [Inject] private Enemy.Enemy.Factory _enemyFactory;

        public void Initialize()
        {
            // Sahnedeki tüm SpawnPoint'leri bul
            _spawnPoints = Object.FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None).ToList();

            // Düşman ölüm sinyaline abone ol
            _signalBus.Subscribe<EnemyDiedSignal>(OnEnemyDied);


            // Başlangıçta tüm spawn noktalarından düşmanları spawn et
            foreach (var sp in _spawnPoints)
            {
               TrySpawnEnemy(sp);
            }

            Debug.Log($"SpawnManager başlatıldı. {_spawnPoints.Count} spawn noktası bulundu.");
        }
        public void Tick()
        {
            // Her spawn noktasını kontrol et ve gerekirse düşman spawn et
            foreach (var sp in _spawnPoints)
            {
                if (sp.CanSpawn)
                {
                  TrySpawnEnemy(sp);
                }
            }
        }

        private void TrySpawnEnemy(SpawnPoint spawnPoint)
        {
            if (spawnPoint.CanSpawn && spawnPoint.EnemyToSpawn != null)
            {
                Enemy.Enemy newEnemy = _enemyFactory.Create(spawnPoint.EnemyToSpawn);
                newEnemy.gameObject.transform.position = spawnPoint.GetAvailableSpawnPosition();
                spawnPoint.AddActiveEnemy(newEnemy);
            }
        }

        private void OnEnemyDied(EnemyDiedSignal signal)
        {
            // Hangi spawn noktasından geldiğini bul ve güncelle
            foreach (var sp in _spawnPoints)
            {
                if (sp.ActiveEnemies.Contains(signal.Enemy))
                {
                    sp.EnemyDied(signal.Enemy);
                    break;
                }
            }
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<EnemyDiedSignal>(OnEnemyDied);
        }
    }
}