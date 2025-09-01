using UnityEngine;

public static class UpgradeFormulas
{
    public static float Linear(int upgradeLevel, float baseValue, float increment)
    {
        float value = baseValue + increment * (upgradeLevel-1);
        return value;
    }

    public static float Logarithmic(int upgradeLevel, float baseValue, float multiplier, float exponent)
    {
        float value = baseValue + multiplier * Mathf.Log(upgradeLevel + 1, exponent);
        return value;
    }

    public static float Constant(float baseValue, int playerLevel = 0, float playerMultiplier = 0f)
    {
        float value = baseValue;
        if (playerMultiplier > 0f && playerLevel > 0)
            value *= 1f + playerMultiplier * playerLevel;
        return value;
    }
    public static float ExponentialPiecewise(int upgradeLevel, float baseValue, float baseRate, float rateIncrease, int periodLength)
    {
        // Hatalı seviye girişi kontrolü
        if (upgradeLevel <= 0)
        {
            return baseValue;
        }

        // Seviyenin hangi periyotta olduğunu bulur.
        int currentPeriod = (upgradeLevel - 1) / periodLength;

        // Bulunan periyoda göre geçerli artış oranını hesaplar.
        float currentRate = baseRate + (currentPeriod * rateIncrease);

        // Mevcut periyotun başındaki seviye değerini hesaplar.
        float startValueForCurrentPeriod = Mathf.Max(1,baseValue);
        for (int i = 0; i < currentPeriod; i++)
        {
            float periodRate = baseRate + (i * rateIncrease);
            startValueForCurrentPeriod *= Mathf.Pow(periodRate, periodLength);
        }
    
        // Mevcut periyot içindeki seviyeyi bulur (0'dan başlar).
        int levelInPeriod = (upgradeLevel - 1) % periodLength;

        // Son değeri, periyot başındaki değere mevcut periyodun artış oranını uygulayarak hesaplar.
        float finalValue = startValueForCurrentPeriod * Mathf.Pow(currentRate, levelInPeriod);
        finalValue = baseValue == 0 && upgradeLevel == 1 ? 0 : finalValue;

        return finalValue;
    }
}