using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HealthBarRenderer : IHealthBarRenderer
{
    private readonly IMaterialProvider _materialProvider;
    private readonly IMeshProvider _meshProvider;
    private readonly string _healthDataPropertyName = "_HealthData";
    private readonly List<Matrix4x4> _matrixList;
    private readonly Queue<MaterialPropertyBlock> _propertyBlockPool;

    public HealthBarRenderer(IMaterialProvider materialProvider, IMeshProvider meshProvider)
    {
        _materialProvider = materialProvider ?? throw new ArgumentNullException(nameof(materialProvider));
        _meshProvider = meshProvider ?? throw new ArgumentNullException(nameof(meshProvider));
        _matrixList = new List<Matrix4x4>();
        _propertyBlockPool = new Queue<MaterialPropertyBlock>();
    }

    private MaterialPropertyBlock GetPropertyBlock()
    {
        if (_propertyBlockPool.Count > 0)
        {
            var block = _propertyBlockPool.Dequeue();
            block.Clear();
            return block;
        }
        return new MaterialPropertyBlock();
    }

    private void ReturnPropertyBlock(MaterialPropertyBlock block)
    {
        block.Clear();
        _propertyBlockPool.Enqueue(block);
    }

    public void DrawHealthBars(IList<Matrix4x4> matrices, Vector4[] healthData)
    {
        if (matrices == null || healthData == null || matrices.Count == 0)
            return;

        var material = _materialProvider.GetHealthBarMaterial();
        var mesh = _meshProvider.GetQuadMesh();

        // IList'i List'e convert et
        _matrixList.Clear();
        for (int i = 0; i < matrices.Count; i++)
        {
            _matrixList.Add(matrices[i]);
        }

        // Object pooling ile MaterialPropertyBlock kullanımı
        var propertyBlock = GetPropertyBlock();
        propertyBlock.SetVectorArray(_healthDataPropertyName, healthData);
        
        Graphics.DrawMeshInstanced(
            mesh,
            0,
            material,
            _matrixList,
            propertyBlock,
            ShadowCastingMode.Off,
            false
        );
        
        ReturnPropertyBlock(propertyBlock);
    }
}