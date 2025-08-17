using System.Collections.Generic;
using Game.Feature.Spawn;
using UnityEngine;
using Zenject;

namespace Game.Feature.UI.HealthBar
{
    public class HealthBarController
    {
        [Inject] SpawnManager spawnManager;

        List<Matrix4x4> matrices = new List<Matrix4x4>();
        List<float> fillAmounts = new List<float>();
        MaterialPropertyBlock mpb;

        void LateUpdate()
        {
            matrices.Clear();
            fillAmounts.Clear();

            foreach(var spawnPoint in spawnManager.)
            {
                foreach(var enemy in spawnPoint.ActiveEnemies)
                {
                    Vector3 pos = enemy.transform.position + Vector3.up * 2f;
                    matrices.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    fillAmounts.Add(enemy.HealthNormalized);
                }
            }

            // Matrices ve fillAmounts ile draw call yapılır
            // ...
        }
    }
}