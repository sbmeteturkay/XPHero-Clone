using System;
using UniRx;
using UnityEngine;

namespace Game.Feature.UI
{

    public class HealthBarController
    {
        private readonly IHealthBarRegistry _healthBarRegistry;
        private readonly HealthBarData _healthBarData=new();
        private readonly Transform _targetTransform;
        private bool _hasHealthBarRegistered;
        private readonly Func<bool> _shouldShowFunc;
        public HealthBarController(IHealthBarRegistry healthBarRegistry,Transform targetTransform, ReactiveProperty<float> currentHealth, ReactiveProperty<float> maxHealth,Func<bool> shouldShowFunc)
        {
            _healthBarRegistry = healthBarRegistry;
            _targetTransform = targetTransform;
            _shouldShowFunc = shouldShowFunc;
            currentHealth.Subscribe(SetCurrentHealth);
            maxHealth.Subscribe(SetMaxHealth);
            UpdateHealthBarValues();
        }

        public void Tick()
        {
            HandleHealthBarDisplay();
        }

        public bool ShouldShowHealthBar()
        {
            return _shouldShowFunc();
        }
        void SetMaxHealth(float health)
        {
            _healthBarData.maxHealth = health;
            UpdateHealthBarValues();
        }

        void SetCurrentHealth(float health)
        {
            _healthBarData.currentHealth = health;
            UpdateHealthBarValues();
        }

        private void UpdateHealthBarValues()
        {
            // Trigger health bar update if it's already registered
            if (_hasHealthBarRegistered)
            {
                UpdateHealthBarPositionAndValues(
                    _targetTransform.position,
                    _healthBarData.GetHealthPercent(),
                    _healthBarData.GetDamagePercent()
                );
            }
        }

        private void HandleHealthBarDisplay()
        {
            if (ShouldShowHealthBar())
            {
                UpdateHealthBar();
            }
            else if (_hasHealthBarRegistered)
            {
                UnregisterHealthBar();
            }
        }

        private void UpdateHealthBar()
        {
            var currentPosition = _targetTransform.position;
            var currentHealthPercent = _healthBarData.GetHealthPercent();
            var currentDamagePercent = _healthBarData.GetDamagePercent();
            if (!_hasHealthBarRegistered)
            {
                RegisterHealthBar(currentPosition, currentHealthPercent, currentDamagePercent);
            }
            UpdateHealthBarPositionAndValues(currentPosition, currentHealthPercent, currentDamagePercent);
        }

        private void RegisterHealthBar(Vector3 position, float healthPercent, float damagePercent)
        {
            if (_healthBarRegistry.RegisterHealthBar(_targetTransform.gameObject, position + Vector3.up * 1.5f, healthPercent, damagePercent))
            {
                _hasHealthBarRegistered = true;
            }
        }

        private void UnregisterHealthBar()
        {
            if (_hasHealthBarRegistered)
            {
                _healthBarRegistry.UnregisterHealthBar(_targetTransform.gameObject);
                _hasHealthBarRegistered = false;
            }
        }

        private void UpdateHealthBarPositionAndValues(Vector3 position, float healthPercent, float damagePercent)
        {
            _healthBarRegistry.UpdateHealthBar(_targetTransform.gameObject, position + Vector3.up * 1.5f, healthPercent, damagePercent);
        }
    }
}