using UnityEngine;

[System.Serializable]
public class UpgradeLevel
{
    public int level;
    public float value; // Для урону та інтервалу
    public GameObject sawPrefab; // Для пилок - кожен рівень = інша пилка
    public int cost;
    
    [TextArea(2, 4)]
    public string levelDescription; // Опис рівня (наприклад "Бронзова пилка")
}

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Upgrades/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Upgrade Settings")]
    public string upgradeName;
    public string description;
    public Sprite icon;
    
    [Header("Upgrade Type")]
    public bool isSawUpgrade = false; // true = використовує sawPrefab, false = використовує value
    
    [Header("Levels")]
    public UpgradeLevel[] levels;
    
    public int maxLevel => levels.Length;
    
    public UpgradeLevel GetLevel(int level)
    {
        if (level < 0 || level >= levels.Length)
            return null;
        return levels[level];
    }
    
    public float GetValue(int level)
    {
        var upgradeLevel = GetLevel(level);
        return upgradeLevel != null ? upgradeLevel.value : 0f;
    }
    
    public GameObject GetSawPrefab(int level)
    {
        var upgradeLevel = GetLevel(level);
        return upgradeLevel != null ? upgradeLevel.sawPrefab : null;
    }
    
    public int GetCost(int level)
    {
        var upgradeLevel = GetLevel(level);
        return upgradeLevel != null ? upgradeLevel.cost : 0;
    }
    
    public string GetLevelDescription(int level)
    {
        var upgradeLevel = GetLevel(level);
        return upgradeLevel != null ? upgradeLevel.levelDescription : "";
    }
}