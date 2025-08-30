using System;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarRegistry : IHealthBarRegistry
{
    private readonly HealthBarSettings _settings;
    private readonly Dictionary<GameObject, HealthBarInstance> _activeHealthBars;
    private readonly List<HealthBarInstance> _healthBarInstances;
    private readonly Queue<int> _availableSlots;
    private readonly Matrix4x4[] _matrices;
    private readonly Vector4[] _healthDataArray;

    internal class HealthBarInstance
    {
        public GameObject enemy;
        public int slotIndex;
        public Vector3 lastPosition;
        public float lastHealthPercent;
        public float lastDamagePercent;
        public bool needsUpdate;
        public bool isActive;

        public HealthBarInstance(int slot)
        {
            slotIndex = slot;
            Reset();
        }

        public void Reset()
        {
            enemy = null;
            isActive = false;
            needsUpdate = false;
            lastHealthPercent = 0;
            lastDamagePercent = 0;
        }
    }

    public HealthBarRegistry(HealthBarSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _activeHealthBars = new Dictionary<GameObject, HealthBarInstance>();
        _healthBarInstances = new List<HealthBarInstance>();
        _availableSlots = new Queue<int>();
        
        _matrices = new Matrix4x4[_settings.maxHealthBars];
        _healthDataArray = new Vector4[_settings.maxHealthBars];

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < _settings.maxHealthBars; i++)
        {
            _healthBarInstances.Add(new HealthBarInstance(i));
            _availableSlots.Enqueue(i);
            _healthDataArray[i] = Vector4.zero;
        }
    }

    public bool RegisterHealthBar(GameObject enemy, Vector3 worldPos, float healthPercent, float damagePercent)
    {
        if (enemy == null)
        {
            Debug.LogError("Cannot register health bar for null enemy");
            return false;
        }

        if (_activeHealthBars.ContainsKey(enemy))
        {
            Debug.LogWarning($"Enemy {enemy.name} already has a health bar registered!");
            return false;
        }

        if (_availableSlots.Count == 0)
        {
            Debug.LogWarning("No available health bar slots!");
            return false;
        }

        int slotIndex = _availableSlots.Dequeue();
        var instance = _healthBarInstances[slotIndex];

        instance.enemy = enemy;
        instance.isActive = true;
        instance.lastPosition = worldPos;
        instance.lastHealthPercent = healthPercent;
        instance.lastDamagePercent = damagePercent;
        instance.needsUpdate = true;

        _activeHealthBars[enemy] = instance;
        return true;
    }

    public bool UpdateHealthBar(GameObject enemy, Vector3 worldPos, float healthPercent, float damagePercent)
    {
        if (!_activeHealthBars.TryGetValue(enemy, out var instance))
            return false;

        bool positionChanged = Vector3.Distance(instance.lastPosition, worldPos) > _settings.positionUpdateThreshold;
        bool healthChanged = Mathf.Abs(instance.lastHealthPercent - healthPercent) > _settings.healthUpdateThreshold;
        bool damageChanged = Mathf.Abs(instance.lastDamagePercent - damagePercent) > _settings.healthUpdateThreshold;

        if (positionChanged || healthChanged || damageChanged)
        {
            instance.lastPosition = worldPos;
            instance.lastHealthPercent = healthPercent;
            instance.lastDamagePercent = damagePercent;
            instance.needsUpdate = true;
        }
        return true;
    }

    public bool UnregisterHealthBar(GameObject enemy)
    {
        if (!_activeHealthBars.TryGetValue(enemy, out var instance))
            return false;

        int slotIndex = instance.slotIndex;
        instance.Reset();

        _matrices[slotIndex] = Matrix4x4.zero;
        _healthDataArray[slotIndex] = Vector4.zero;

        _activeHealthBars.Remove(enemy);
        _availableSlots.Enqueue(slotIndex);

        return true;
    }

    public void ClearAllHealthBars()
    {
        foreach (var instance in _activeHealthBars.Values)
        {
            instance.Reset();
            _availableSlots.Enqueue(instance.slotIndex);
        }

        _activeHealthBars.Clear();

        for (int i = 0; i < _settings.maxHealthBars; i++)
        {
            _matrices[i] = Matrix4x4.zero;
            _healthDataArray[i] = Vector4.zero;
        }
    }

    public int GetActiveCount() => _activeHealthBars.Count;
    public int GetAvailableSlotCount() => _availableSlots.Count;

    // Internal methods for HealthBarManager
    internal Dictionary<GameObject, HealthBarInstance> GetActiveInstances() => _activeHealthBars;
    internal Matrix4x4[] GetMatrices() => _matrices;
    internal Vector4[] GetHealthDataArray() => _healthDataArray;
    internal HealthBarSettings GetSettings() => _settings;
}