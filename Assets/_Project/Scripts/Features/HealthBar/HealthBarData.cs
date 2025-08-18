using UnityEngine;

[System.Serializable]
public class HealthBarData
{
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float maxHealth;
    [HideInInspector] public float lastDamageAmount;
    float damageDisplayTimer;
    float damageDisplayDuration = .2f;
    
    public void TakeDamage(float damage)
    {
        lastDamageAmount = damage;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        damageDisplayTimer = damageDisplayDuration;
    }
    
    public void UpdateTimer(float deltaTime)
    {
        if (damageDisplayTimer > 0)
            damageDisplayTimer -= deltaTime;
    }
    
    public float GetHealthPercent() => currentHealth / maxHealth;
    public float GetDamagePercent() => damageDisplayTimer > 0 ? lastDamageAmount / maxHealth : 0f;
}