using UnityEngine;

[CreateAssetMenu(menuName = "Game/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    [Header("Info")]
    public Sprite icon;
    public string upgradeName;
    [TextArea] public string description;

    [Header("Level Data")]
    public int maxLevel = 100;
}