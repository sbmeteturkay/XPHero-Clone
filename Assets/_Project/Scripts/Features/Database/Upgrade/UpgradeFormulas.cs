using UnityEngine;

public static class UpgradeFormulas
{
    public static float Linear(int upgradeLevel, float baseValue, float increment, int playerLevel = 0, float playerMultiplier = 0f)
    {
        float value = baseValue + increment * upgradeLevel;
        if (playerMultiplier > 0f && playerLevel > 0)
            value *= 1f + playerMultiplier * playerLevel;
        return value;
    }

    public static float Logarithmic(int upgradeLevel, float baseValue, float multiplier, float exponent, int playerLevel = 0, float playerMultiplier = 0f)
    {
        float value = baseValue + multiplier * Mathf.Log(upgradeLevel + 1, exponent);
        if (playerMultiplier > 0f && playerLevel > 0)
            value *= 1f + playerMultiplier * playerLevel;
        return value;
    }

    public static float Constant(float baseValue, int playerLevel = 0, float playerMultiplier = 0f)
    {
        float value = baseValue;
        if (playerMultiplier > 0f && playerLevel > 0)
            value *= 1f + playerMultiplier * playerLevel;
        return value;
    }
}