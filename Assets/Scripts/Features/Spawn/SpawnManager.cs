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
            //TODO: use  LINQ methods 
            foreach (var spawnPoint in _spawnPoints)
            {
                if (spawnPoint.gameObject == obj.SpawnArea.gameObject)
                {
                    spawnPoint.EnemyCanChaseOrAttack = false;
                }
            }
        }

        private void OnPlayerEnteredSpawnArea(PlayerEnteredSpawnAreaSignal obj)
        {
            //TODO: use  LINQ methods 
            foreach (var spawnPoint in _spawnPoints)
            {
                if (spawnPoint.gameObject == obj.SpawnArea.gameObject)
                {
                    spawnPoint.EnemyCanChaseOrAttack = true;
                }
            }
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
        }


        public void Dispose()
        {
            _signalBus.Unsubscribe<PlayerEnteredSpawnAreaSignal>(OnPlayerEnteredSpawnArea);
            _signalBus.Unsubscribe<PlayerExitedSpawnAreaSignal>(OnPlayerExitedSpawnArea);
        }
    }
}