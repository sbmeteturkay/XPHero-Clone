using System.Linq;
using Game.Core.Services;
using Game.Feature.Enemy;
using Game.Feature.Player;
using Game.Feature.Spawn;
using UnityEngine;
using Zenject;

namespace Game.Core.Installer
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [Inject] InputService inputService;
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayerService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SpawnManager>().AsSingle().NonLazy();

            Container.Bind<DamageService>().AsSingle();
            
            Container.DeclareSignal<EnemyTookDamageSignal>();
            Container.DeclareSignal<DamageAppliedSignal>();
            
            Container.DeclareSignal<PlayerTookDamageSignal>();
            Container.DeclareSignal<PlayerDiedSignal>();
            Container.DeclareSignal<PlayerHealedSignal>();
            
            Container.DeclareSignal < PlayerEnteredSpawnAreaSignal>();
            Container.DeclareSignal < PlayerExitedSpawnAreaSignal>();
            
            SpawnEnemies();
             
             inputService.EnablePlayerInput();
        }

        private void SpawnEnemies()
        {
            var spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None)
                .Where(sp => sp.EnemyToSpawn != null && sp.EnemyToSpawn.EnemyPrefab != null)
                .ToList();

// EnemyData - SpawnPoint ikililerini al
            var enemyDataWithSpawnPoints = spawnPoints
                .Select(sp => (EnemyData: sp.EnemyToSpawn, SpawnPoint: sp))
                .ToList();

// Unique olanları almak için EnemyData’ya göre grupla
            var uniqueEnemyDataWithSpawnPoints = enemyDataWithSpawnPoints
                .GroupBy(x => x.EnemyData)
                .Select(g => (EnemyData: g.Key, SpawnPoint: g.First().SpawnPoint))
                .ToList();

            // Her benzersiz EnemyData'nın prefab'ı için ayrı bir Factory ve MemoryPool bind et
            foreach (var enemyData in uniqueEnemyDataWithSpawnPoints)
            {
                Container.BindFactory<EnemyData, Enemy, Enemy.Factory>()
                    .FromPoolableMemoryPool(poolBinder => poolBinder
                        .WithInitialSize(enemyData.SpawnPoint.MaxEnemies) // Başlangıç boyutu, oyunun ihtiyacına göre ayarlanmalı
                        .FromComponentInNewPrefab(enemyData.EnemyData.EnemyPrefab)
                        .UnderTransform(enemyData.SpawnPoint.transform));
            }

            // EnemyState'leri bind et (Patrol, Chase, Attack, Idle)
            Container.Bind<IEnemyState>().To<EnemyIdleState>().AsTransient();
            Container.Bind<IEnemyState>().To<EnemyPatrolState>().AsTransient();
            Container.Bind<IEnemyState>().To<EnemyChaseState>().AsTransient();
            Container.Bind<IEnemyState>().To<EnemyAttackState>().AsTransient();

            // EnemyMovement ve EnemyAttack gibi davranış bileşenlerini bind et
            // Bunlar MonoBehaviour olduğu için, Zenject'in GameObjectContext veya SceneContext'i tarafından
            // otomatik olarak inject edilebilirler veya manuel olarak bind edilebilirler.
            // Eğer düşman prefab'ı üzerinde zaten varsa, Zenject otomatik olarak inject edecektir.
            // Eğer dinamik olarak eklenecekse, aşağıdaki gibi bind edilebilir:
            // Container.Bind<EnemyMovement>().FromNewComponentOnNewGameObject().AsTransient();
            // Container.Bind<EnemyAttack>().FromNewComponentOnNewGameObject().AsTransient();
        }

        void OnDisable()
        {
            inputService.DisablePlayerInput();
        }
    }
}