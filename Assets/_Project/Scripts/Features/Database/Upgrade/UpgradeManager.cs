using UnityEngine;
using System.Collections.Generic;
using UniRx;
using Zenject;

public class UpgradeData
{
    public int Power=>PlayerPrefs.GetInt("Upgrade_POWER_Value");
    public int CargoCapacity;
    public int HP=>PlayerPrefs.GetInt("Upgrade_HP_Value");
    public int HPRecovery=>PlayerPrefs.GetInt("Upgrade_HP RECOVERY_Value");
    public int MoveSpeed=>PlayerPrefs.GetInt("Upgrade_MOVE SPEED_Value");
    public float CritChange;
    public float AttackSpeed=>PlayerPrefs.GetInt("Upgrade_ATTACK SPEED_Value");
    public float DoubleAttackChange;
    public float TripleAttack;
}
public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private UpgradeUnitUI upgradePrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private List<UpgradeSO> upgrades;
    private readonly Dictionary<UpgradeUnitUI, UpgradeUnit> upgradesDict = new();
    [Inject] public ReactiveProperty<UpgradeData> PlayerUpgradeData;

    private void Awake()
    {
        LoadUpgrades();
    }

    private void LoadUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            string key = "Upgrade_" + upgrade.upgradeName;
            var upgradeUnitUI = Instantiate(upgradePrefab, contentParent);
            upgradeUnitUI.SetData(upgrade);
            upgradeUnitUI.onButtonClicked = () =>
            {
                TryUpgrade(upgradeUnitUI);
            };
            var unit = new UpgradeUnit(key,upgrade);
            
            upgradesDict.Add(upgradeUnitUI,unit);
            upgradeUnitUI.Refresh(unit);
        }
    }

    void TryUpgrade(UpgradeUnitUI upgradeUnitUI)
    {
        var upgrade = upgradesDict[upgradeUnitUI];
        var canBuy = true;
        if (canBuy)
        {
            upgrade.Upgrade();
            PlayerUpgradeData.SetValueAndForceNotify(PlayerUpgradeData.Value);
        }
        upgradeUnitUI.Refresh(upgrade);
    }
}

public class UpgradeUnit
{
    public bool isPercentage;
    public int level;
    public string key;
    private float baseValue;
    private int basePrice;
    
    private float baseRate;
    private float baseRateIncrease;
    private int periodLenght;

    public UpgradeUnit(string key, UpgradeSO upgrade)
    {
        baseValue = upgrade.baseValue;
        basePrice = upgrade.basePrice;
        baseRate= upgrade.baseRate;
        baseRateIncrease= upgrade.rateIncrease;
        periodLenght=upgrade.periodLength;
        isPercentage =upgrade.isPercentage;
        
        if(!PlayerPrefs.HasKey(key)){
            PlayerPrefs.SetInt(key,1);
            PlayerPrefs.Save();
        }
        level = PlayerPrefs.GetInt(key);
        PlayerPrefs.SetInt(key+"_Value",GetValue());
        this.key = key;

    }

    public void Upgrade()
    {
        level++;
        PlayerPrefs.SetInt(key, level);
        PlayerPrefs.SetInt(key+"_Value",GetValue());
        PlayerPrefs.Save();
    }
    public int GetValue()
    {
        return (int)UpgradeFormulas.ExponentialPiecewise(
            upgradeLevel: level,
            baseValue: baseValue,
            baseRate: baseRate,
            rateIncrease: baseRateIncrease,
            periodLength: periodLenght
        );
    }

    public float GetNextValue()
    {
        return UpgradeFormulas.ExponentialPiecewise(
            upgradeLevel: level+1,
            baseValue: baseValue,
            baseRate: baseRate,
            rateIncrease: baseRateIncrease,
            periodLength: periodLenght
        );
    }

    public int GetCost()
    {
        return (int)UpgradeFormulas.ExponentialPiecewise(
            upgradeLevel: level,
            baseValue: basePrice,
            baseRate: 1.15f,
            rateIncrease: 0.01f,
            periodLength: 20
        );
    }
}
