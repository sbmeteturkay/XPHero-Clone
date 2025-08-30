using System;
using System.Collections.Generic;
using UnityEngine;

// Configuration
[Serializable]
public class HealthBarSettings
{
    public float width = 2f;
    public float height = 0.3f;
    public float offsetY = 1.5f;
    public int maxHealthBars = 100;
    public float positionUpdateThreshold = 0.01f;
    public float healthUpdateThreshold = 0.001f;
}

// Interfaces for dependency inversion
public interface IHealthBarRenderer
{
    void DrawHealthBars(IList<Matrix4x4> matrices, Vector4[] healthData);
}

public interface IMeshProvider
{
    Mesh GetQuadMesh();
}

public interface IMaterialProvider
{
    Material GetHealthBarMaterial();
}

public interface ICameraProvider
{
    Camera GetMainCamera();
}

public interface IHealthBarRegistry
{
    bool RegisterHealthBar(GameObject enemy, Vector3 worldPos, float healthPercent, float damagePercent);
    bool UpdateHealthBar(GameObject enemy, Vector3 worldPos, float healthPercent, float damagePercent);
    bool UnregisterHealthBar(GameObject enemy);
    void ClearAllHealthBars();
    int GetActiveCount();
    int GetAvailableSlotCount();
}

// Implementations
public class MeshProvider : IMeshProvider
{
    private Mesh _quadMesh;

    public Mesh GetQuadMesh()
    {
        if (_quadMesh == null)
        {
            CreateQuadMesh();
        }
        return _quadMesh;
    }

    private void CreateQuadMesh()
    {
        _quadMesh = new Mesh { name = "HealthBar Quad" };

        Vector3[] vertices = {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };

        Vector2[] uvs = {
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)
        };

        int[] triangles = { 0, 2, 1, 2, 3, 1 };
        Vector3[] normals = { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };

        _quadMesh.vertices = vertices;
        _quadMesh.uv = uvs;
        _quadMesh.triangles = triangles;
        _quadMesh.normals = normals;
    }
}

public class CameraProvider : ICameraProvider
{
    private Camera _mainCamera;

    public Camera GetMainCamera()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
        return _mainCamera;
    }
}