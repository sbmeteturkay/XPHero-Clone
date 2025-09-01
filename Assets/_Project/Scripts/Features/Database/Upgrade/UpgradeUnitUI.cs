using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class UpgradeUnitUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private TMP_Text nextValueText;
    
    [SerializeField] private TMP_Text level;
    
    [SerializeField] private TMP_Text costText;
    
    [SerializeField] private Button upgradeButton;
    
    public delegate void UpgradeDelegate();
    public UpgradeDelegate onButtonClicked;
    private int maxLevel;

    public void SetData(UpgradeSO data)
    {
        icon.sprite = data.icon;
        nameText.text = data.name;
        descriptionText.text = data.description;
        
        maxLevel=data.maxLevel;
        
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    public void Refresh(UpgradeUnit upgrade)
    {
        string prefix = upgrade.isPercentage ? "%" : "";
        currentValueText.text = prefix + FormatValue(upgrade.GetValue(), upgrade.isPercentage);
        nextValueText.text    = prefix + FormatValue(upgrade.GetNextValue(), upgrade.isPercentage);
        costText.text         = FormatValue(upgrade.GetCost(), false);

        level.text = "Level "+upgrade.level + "/" +(maxLevel);
    }

    private void OnUpgradeClicked()
    {
        onButtonClicked?.Invoke();
    }
    public static string FormatValue(float value, bool isPercentage)
    {
        if (isPercentage)
        {
            // Percentage için 3 basamaktan uzun olmayacak şekilde format
            string format;
            if (value >= 100f) format = "0";       // 100-999 → 3 basamak
            else if (value >= 10f) format = "0.#"; // 10-99.9 → 2-3 basamak
            else format = "0.##";                  // 0-9.99 → 1-3 basamak
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
        else
        {
            if (value < 1000f)
                return ((int)value).ToString(CultureInfo.InvariantCulture); // 3 hane olana kadar noktasız
            else
            {
                // Büyük değerleri kısalt
                string[] suffixes = { "", "K", "M", "B" };
                int suffixIndex = 0;
                float v = value;
                while (v >= 1000f && suffixIndex < suffixes.Length - 1)
                {
                    v /= 1000f;
                    suffixIndex++;
                }

                string format;
                if (v >= 100f) format = "0";       // 100-999 → 3 basamak
                else if (v >= 10f) format = "0.#"; // 10-99.9 → 2-3 basamak
                else format = "0.##";              // 0-9.99 → 1-3 basamak

                return v.ToString(format, CultureInfo.InvariantCulture) + suffixes[suffixIndex];
            }
        }
    }
}
