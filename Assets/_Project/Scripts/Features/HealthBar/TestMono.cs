using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Feature.UI
{
    public class TestMono : MonoBehaviour
    {
        public Mesh quadMesh;
        public Material healthBarMaterial;

        const int instanceCount = 5;
        Matrix4x4[] matrices = new Matrix4x4[instanceCount];
        float[] fills = new float[instanceCount];
        MaterialPropertyBlock mpb;

        void Start()
        {
            mpb = new MaterialPropertyBlock();

            // Farklı pozisyonlar ve farklı doluluk oranları
            for (int i = 0; i < instanceCount; i++)
            {
                Vector3 pos = new Vector3(i * 2f, 1f, 0);
                matrices[i] = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);

                // Doluluk oranları: 0.2, 0.4, 0.6, 0.8, 1.0
                fills[i] = (i + 1) * 0.2f;
            }
        }

        void Update()
        {
            // Per-instance _Fill değerini gönder
            mpb.Clear();
            mpb.SetFloatArray("_Fill", fills);

            Graphics.DrawMeshInstanced(quadMesh, 0, healthBarMaterial, matrices, instanceCount, mpb,
                ShadowCastingMode.Off, false, 0);
        }
    }
}