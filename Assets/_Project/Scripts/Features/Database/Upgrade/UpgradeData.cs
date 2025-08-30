using UnityEngine;

[CreateAssetMenu(menuName = "Game/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    [Header("Info")]
    public Sprite icon;
    public string upgradeName;
    [TextArea] public string description;

    [Header("Editable Data")]
    public int maxLevel = 100;

    public float baseValue = 1;
    public int basePrice=10;
    public float baseRate= 1.25f;
    public float rateIncrease= -0.01f;
    public int periodLength= 20;

}