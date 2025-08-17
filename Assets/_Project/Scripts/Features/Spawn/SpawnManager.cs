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
        [Inject] private Enemy.Enemy.Factory _enemyFactory;

        private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        public List<SpawnPoint> playerInsideSpawnPoints = new List<SpawnPoint>();

        public void Initialize()
        {
            // Sahnedeki tüm SpawnPoint'leri bul
            _spawnPoints = Object.FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None).ToList();

            // Düşman ölüm sinyaline abone ol
            _signalBus.Subscribe<PlayerEnteredSpawnAreaSignal>(OnPlayerEnteredSpawnArea);
            _signalBus.Subscribe<PlayerExitedSpawnAreaSignal>(OnPlayerExitedSpawnArea);

            // Başlangıçta tüm spawn noktalarından düşmanları spawn et
            foreach (var sp in _spawnPoints)
            {
                while (sp.MaxEnemies>sp.ActiveEnemies.Count)
                {
                    CreateEnemy(sp);
                }
            }
        }

        private void OnPlayerExitedSpawnArea(PlayerExitedSpawnAreaSignal obj)
        {
            var point = _spawnPoints.Find(x => x.gameObject == obj.SpawnArea.gameObject);
            playerInsideSpawnPoints.Remove(point);
            point.PlayerInsideSpawnPoint=false;
        }

        private void OnPlayerEnteredSpawnArea(PlayerEnteredSpawnAreaSignal obj)
        {
            var point = _spawnPoints.Find(x => x.gameObject == obj.SpawnArea.gameObject);
            playerInsideSpawnPoints.Add(point);
            point.PlayerInsideSpawnPoint=true;
        }

        public List<Enemy.Enemy> GetTargetableEnemies()
        {
            List<Enemy.Enemy> targetables = new List<Enemy.Enemy>();
            foreach (SpawnPoint spawnPoint in _spawnPoints)
            {
                targetables.AddRange(spawnPoint.ActiveEnemies.FindAll(x=>x.Targetable));
            }
            return targetables;
        } 
        public void Tick()
        {
            foreach (var spawnPoint in _spawnPoints)
            {
                if (spawnPoint.ShouldSpawnEnemy)
                {
                    TrySpawnEnemy(spawnPoint);
                }
            }
        }

        private void TrySpawnEnemy(SpawnPoint spawnPoint)
        {
            var newEnemy = spawnPoint.EnemiesToRespawn.Dequeue();
            newEnemy.gameObject.transform.position = spawnPoint.GetAvailableSpawnPosition();
            newEnemy.SetSpawnPoint(spawnPoint);
            newEnemy.Respawn(spawnPoint.EnemyToSpawn);
            spawnPoint.AddActiveEnemy(newEnemy);
        }

        private void CreateEnemy(SpawnPoint spawnPoint)
        {
            var newEnemy = _enemyFactory.Create(spawnPoint.EnemyToSpawn);
            newEnemy.gameObject.transform.position = spawnPoint.GetAvailableSpawnPosition();
            newEnemy.SetSpawnPoint(spawnPoint);
            spawnPoint.AddActiveEnemy(newEnemy);
            spawnPoint.EnemiesToRespawn.Enqueue(newEnemy);
        }


        public void Dispose()
        {
            _signalBus.Unsubscribe<PlayerEnteredSpawnAreaSignal>(OnPlayerEnteredSpawnArea);
            _signalBus.Unsubscribe<PlayerExitedSpawnAreaSignal>(OnPlayerExitedSpawnArea);
        }
    }
}