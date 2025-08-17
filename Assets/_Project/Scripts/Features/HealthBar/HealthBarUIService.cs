using System;
using System.Collections.Generic;
using Game.Feature.Spawn;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Game.Feature.UI
{
    public class HealthBarUIService : IInitializable, ITickable
    {
        [Header("Mesh & Materials")]
        Mesh quadMesh;
        Material backMaterial; // white background (instancing enabled)
        Material fillMaterial; // red fill (instancing enabled)
        [Header("Size & Offset")]
        Vector2 size = new Vector2(1f, 0.2f); // width, height in world units
        float yOffset = 1.0f;
         
        private MaterialPropertyBlock mpb;
        private List<Matrix4x4> matrices = new List<Matrix4x4>(50);
        private List<float> fillAmounts = new List<float>(50);
        List<Matrix4x4> bgMatrices = new List<Matrix4x4>(64);
        List<Matrix4x4> fillMatrices = new List<Matrix4x4>(64);

        // SpawnManager referansı Zenject ile inject edilebilir
        [Inject] SpawnManager spawnManager;

        public HealthBarUIService(Mesh quadMesh, Material backMaterial, Material fillMaterial)
        {
            this.quadMesh = quadMesh;
            this.backMaterial = backMaterial;
            this.fillMaterial = fillMaterial;
        }

        private const int MaxInstancesPerBatch = 1023;

        public void Initialize()
        {
            mpb = new MaterialPropertyBlock();
        }

        
        public void Tick()
        {
            bgMatrices.Clear();
            fillMatrices.Clear();

            if (spawnManager == null)
                return;

            foreach (var sp in spawnManager.playerInsideSpawnPoints)
            {
                foreach (var enemy in sp.ActiveEnemies)
                {
                    if (enemy == null) continue;

                    Vector3 pos = enemy.transform.position + Vector3.up * yOffset;

                    // Background: full size
                    bgMatrices.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));

                    // Fill: scale.x = health percent (0..1), others = 1
                    float fill = Mathf.Clamp01(enemy.CurrentHealthPercent);
                    fillMatrices.Add(Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(fill, 1f, 1f)));
                }
            }

            // Set global per-frame properties
            Vector3 camPos = Camera.main.transform.position;
            mpb.Clear();
            mpb.SetVector("_WorldSpaceCameraPos", new Vector4(camPos.x, camPos.y, camPos.z, 1f));
            mpb.SetVector("_Size", new Vector4(1, .2f, 0, 0));

            DrawBatched(quadMesh, backMaterial, bgMatrices);
            DrawBatched(quadMesh, fillMaterial, fillMatrices);
        }

        void DrawBatched(Mesh mesh, Material mat, List<Matrix4x4> matrices)
        {
            int count = matrices.Count;
            int offset = 0;

            while (offset < count)
            {
                int batchCount = Mathf.Min(MaxInstancesPerBatch, count - offset);
                var arr = matrices.GetRange(offset, batchCount).ToArray();

                Graphics.DrawMeshInstanced(mesh, 0, mat, arr, batchCount, mpb,
                    ShadowCastingMode.Off, false, 0);

                offset += batchCount;
            }
        }
    }
}