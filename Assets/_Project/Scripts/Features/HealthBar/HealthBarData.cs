using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class HealthBarData
{
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float maxHealth;
    [HideInInspector] public float lastDamageAmount;
    float damageDisplayTimer;
    
    public void TakeDamage(float damage)
    {
        lastDamageAmount = damage;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        damageDisplayTimer = .5f;
    }
    
    public void UpdateTimer(float deltaTime)
    {
        if (damageDisplayTimer > 0)
            damageDisplayTimer -= deltaTime;
    }
    
    public float GetHealthPercent() => currentHealth / maxHealth;
    public float GetDamagePercent() => damageDisplayTimer > 0 ? math.remap(0,.5f,0,lastDamageAmount / maxHealth,damageDisplayTimer) : 0f;
}