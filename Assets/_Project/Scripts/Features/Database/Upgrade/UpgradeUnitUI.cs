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
        currentValueText.text = upgrade.GetValue().ToString();
        nextValueText.text =  upgrade.GetNextValue().ToString();
        costText.text = upgrade.GetCost().ToString();
        level.text = "Level "+upgrade.level + "/" +(maxLevel);
    }

    private void OnUpgradeClicked()
    {
        onButtonClicked?.Invoke();
    }
}