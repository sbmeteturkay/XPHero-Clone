using UnityEngine;
using System.Collections.Generic;
using Game.Feature.Enemy;

namespace Game.Feature.Spawn
{
    public class SpawnPoint : MonoBehaviour
    {
        public EnemyData EnemyToSpawn; // Bu spawn noktasından doğacak düşman tipi
        public int MaxEnemies = 3; // Bu noktadan aynı anda doğabilecek maksimum düşman sayısı
        public float RespawnTime = 5f; // Düşman öldükten sonra tekrar doğma süresi
        public Vector2 areaSize = Vector2.one*2;
        public LayerMask enemyLayerMask;

        [HideInInspector] public List<Enemy.Enemy> ActiveEnemies = new List<Enemy.Enemy>();
        private float _respawnTimer;

        public bool CanSpawn => ActiveEnemies.Count < MaxEnemies && _respawnTimer <= 0;
        public bool EnemyCanChaseOrAttack;
        public void EnemyDied(Enemy.Enemy enemy)
        {
            if (ActiveEnemies.Contains(enemy))
            {
                ActiveEnemies.Remove(enemy);
                _respawnTimer = RespawnTime;
            }
        }

        public void AddActiveEnemy(Enemy.Enemy enemy)
        {
            if (!ActiveEnemies.Contains(enemy))
            {
                ActiveEnemies.Add(enemy);
            }
        }
        public Vector3 GetAvailableSpawnPosition()
        {
            const int maxAttempts = 10;
            float checkRadius = 0.5f; // enemy boyutuna göre ayarla

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 randomOffset = new Vector2(
                    Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                    Random.Range(-areaSize.y / 2f, areaSize.y / 2f)
                );

                Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, 0f, randomOffset.y);
                bool occupied = Physics.CheckSphere(spawnPos, checkRadius, enemyLayerMask);

                if (!occupied)
                    return spawnPos;
            }

            return transform.position; // uygun yer bulunamadı
        }
        void Update()
        {
            if (_respawnTimer > 0)
            {
                _respawnTimer -= Time.deltaTime;
            }
        }
        
    }
}
