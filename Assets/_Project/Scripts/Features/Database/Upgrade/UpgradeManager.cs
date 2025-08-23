using UnityEngine;
using System.Collections.Generic;


public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private UpgradeUnitUI upgradePrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private List<UpgradeSO> upgrades;
    Dictionary<UpgradeUnitUI, UpgradeUnit> upgradesDict = new Dictionary<UpgradeUnitUI, UpgradeUnit>();
    
    private void Awake()
    {
        LoadUpgrades();
    }

    private void LoadUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            string key = "Upgrade_" + upgrade.upgradeName;
            var ui = Instantiate(upgradePrefab, contentParent);
            ui.SetData(upgrade);
            ui.onButtonClicked = () =>
            {
                TryUpgrade(ui);
            };
            var unit = new UpgradeUnit(0);
            
            upgradesDict.Add(ui,unit);
            ui.Refresh(unit);
        }
    }

    void TryUpgrade(UpgradeUnitUI key)
    {
        var upgrade = upgradesDict[key];
        var canBuy = true;
        if (canBuy)
        {
            upgrade.level++;
        }
        key.Refresh(upgrade);
    }

    private int UpgradeCostValueRefresh()
    {
        return (int)UpgradeFormulas.Logarithmic(1, 1,2,2);
    }

    private int UpgradeCostRefresh()
    {
        return (int)UpgradeFormulas.Constant(1, 1);
    }
}

public class UpgradeUnit
{
    public int level;

    public UpgradeUnit(int level)
    {
        this.level = level;
    }

    public int GetValue()
    {
        return (int)UpgradeFormulas.Linear(level, 1, 1, level);
    }

    public int GetNextValue()
    {
        return (int)UpgradeFormulas.Linear(level, 1, 1, level+1);
    }

    public int GetCost()
    {
        return (int)UpgradeFormulas.Linear(level,1,1,1);
    }
}
