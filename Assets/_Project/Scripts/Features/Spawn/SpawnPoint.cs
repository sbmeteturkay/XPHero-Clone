using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.Feature.Enemy;
using R3;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Game.Feature.Spawn
{
    public class SpawnPoint : MonoBehaviour
    {
        public EnemyData EnemyToSpawn; // Bu spawn noktasından doğacak düşman tipi
        public int MaxEnemies = 3; // Bu noktadan aynı anda doğabilecek maksimum düşman sayısı
        public float radius = 2;
        public LayerMask enemyLayerMask;

        [HideInInspector] public List<Enemy.Enemy> ActiveEnemies = new ();
        [HideInInspector] public List<Enemy.Enemy> DeactiveEnemies = new ();
        [HideInInspector] public Queue<Enemy.Enemy> EnemiesToRespawn = new ();

        public bool ShouldSpawnEnemy => EnemiesToRespawn.Count>0;
        public bool PlayerInsideSpawnPoint;

        private void Update()
        {
            foreach (var enemy in DeactiveEnemies.ToList())
            {
                enemy.UpdateWhileDeactivated();
            }
        }

        public void EnemyDied(Enemy.Enemy enemy)
        {
            DeactiveEnemies.Add(enemy);
            enemy.CanSpawnSubscription=enemy.CanSpawn.Subscribe(x =>
            {
                if (x && !EnemiesToRespawn.Contains(enemy))
                {
                    DeactiveEnemies.Remove(enemy);
                    EnemiesToRespawn.Enqueue(enemy);
                    enemy.CanSpawnSubscription.Dispose();
                }
            });
        }

        public void AddActiveEnemy(Enemy.Enemy enemy)
        {
            if (!ActiveEnemies.Contains(enemy))
                ActiveEnemies.Add(enemy);
        }

        public Vector3 GetAvailableSpawnPosition()
        {
            const int maxAttempts = 10;
            float checkRadius = 0.5f; // enemy boyutuna göre ayarla

            for (int i = 0; i < maxAttempts; i++)
            {
                float angle = Random.Range(0f, 360f);
                // Random bir mesafe seç (0 - radius)
                float distance = Random.Range(0f, radius);

                // Açıyı radyana çevir
                float rad = angle * Mathf.Deg2Rad;

                // X ve Z bileşenlerini hesapla
                float x = Mathf.Cos(rad) * distance;
                float z = Mathf.Sin(rad) * distance;

                Vector3 spawnPos = transform.position + new Vector3(x, 0f, z);
                bool occupied = Physics.CheckSphere(spawnPos, checkRadius, enemyLayerMask);

                if (!occupied)
                    return spawnPos;
            }

            return transform.position; // uygun yer bulunamadı
        }

        public Vector3 GetRandomPositionInRadius()
        {
            float angle = Random.Range(0f, 360f);
            // Random bir mesafe seç (0 - radius)
            float distance = Random.Range(0f, radius);

            // Açıyı radyana çevir
            float rad = angle * Mathf.Deg2Rad;

            // X ve Z bileşenlerini hesapla
            float x = Mathf.Cos(rad) * distance;
            float z = Mathf.Sin(rad) * distance;

            Vector3 spawnPos = transform.position + new Vector3(x, 0f, z);
            return spawnPos;
        }
        
        private void OnDrawGizmos()
        {
            int segments = 60;
            Vector3 center = transform.position;

            Gizmos.color = new Color(1, 1, 1, 0.3f); // Saydam beyaz

            Vector3 previousPoint = center + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * 2 * Mathf.PI / segments;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                // Tırtıklı çizgi için segmentleri atla
                if (i % 2 == 0)
                    Gizmos.DrawLine(previousPoint, newPoint);

                previousPoint = newPoint;
            }
        }
    }
}
