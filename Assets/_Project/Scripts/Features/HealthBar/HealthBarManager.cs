using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class HealthBarManager : IInitializable, ILateTickable, IDisposable
{
    private readonly HealthBarRegistry _registry;
    private readonly IHealthBarRenderer _renderer;
    private readonly ICameraProvider _cameraProvider;
    private readonly List<Matrix4x4> _activeMatrices;
    private readonly List<Vector4> _activeHealthData;

    public HealthBarManager(
        HealthBarRegistry registry,
        IHealthBarRenderer renderer,
        ICameraProvider cameraProvider)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _cameraProvider = cameraProvider ?? throw new ArgumentNullException(nameof(cameraProvider));
        
        _activeMatrices = new List<Matrix4x4>();
        _activeHealthData = new List<Vector4>();
    }

    public void Initialize()
    {
        // Initialization logic if needed
    }

    public void LateTick()
    {
        var activeInstances = _registry.GetActiveInstances();
        if (activeInstances.Count == 0) return;

        var camera = _cameraProvider.GetMainCamera();

        var cameraPos = camera.transform.position;
        var matrices = _registry.GetMatrices();
        var healthDataArray = _registry.GetHealthDataArray();
        var settings = _registry.GetSettings();

        _activeMatrices.Clear();
        _activeHealthData.Clear();

        foreach (var kvp in activeInstances)
        {
            var instance = kvp.Value;
            if (!instance.isActive) continue;

            var targetPos = instance.lastPosition + Vector3.up * settings.offsetY;
            var direction = (targetPos - cameraPos).normalized;
            var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            var rotation = Quaternion.Euler(0, angle, 0);
            var scale = new Vector3(settings.width, settings.height, 1);

            var matrix = Matrix4x4.TRS(targetPos, rotation, scale);
            matrices[instance.slotIndex] = matrix;

            var healthData = new Vector4(
                instance.lastHealthPercent,
                instance.lastDamagePercent,
                0, 0
            );
            healthDataArray[instance.slotIndex] = healthData;

            if (instance.needsUpdate)
            {
                _activeMatrices.Add(matrix);
                _activeHealthData.Add(healthData);
                instance.needsUpdate = false;
            }
        }

        if (_activeMatrices.Count > 0)
        {
            _renderer.DrawHealthBars(_activeMatrices, _activeHealthData.ToArray());
        }
    }

    public void Dispose()
    {
        _registry.ClearAllHealthBars();
    }
}