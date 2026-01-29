using UnityEngine;
using System;

public class PlayerUpgradeManager : MonoBehaviour
{
    public static PlayerUpgradeManager Instance { get; private set; }
    
    [Header("Upgrade Data")]
    public UpgradeData damagePerSecondUpgrade;
    public UpgradeData hitIntervalUpgrade;
    public UpgradeData sawCountUpgrade;
    
    [Header("Current Stats")]
    [SerializeField] private int damagePerSecondLevel = 0;
    [SerializeField] private int hitIntervalLevel = 0;
    [SerializeField] private int sawCountLevel = 0;
    [SerializeField] private int playerMoney = 1000; // Стартові гроші
    
    // Events для оновлення UI
    public event Action OnUpgradeChanged;
    public event Action<int> OnMoneyChanged;
    
    // Публічні властивості
    public float CurrentDamagePerSecond => damagePerSecondUpgrade.GetValue(damagePerSecondLevel);
    public float CurrentHitInterval => hitIntervalUpgrade.GetValue(hitIntervalLevel);
    public GameObject CurrentSawPrefab => sawCountUpgrade.GetSawPrefab(sawCountLevel); // Повертає GameObject пилки
    
    public int DamagePerSecondLevel => damagePerSecondLevel;
    public int HitIntervalLevel => hitIntervalLevel;
    public int SawCountLevel => sawCountLevel;
    public int Money => playerMoney;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadUpgrades();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public bool CanAffordUpgrade(UpgradeData upgradeData, int currentLevel)
    {
        if (currentLevel >= upgradeData.maxLevel)
            return false;
        
        int cost = upgradeData.GetCost(currentLevel);
        return playerMoney >= cost;
    }
    
    public bool TryUpgrade(UpgradeType type)
    {
        UpgradeData data = null;
        int currentLevel = 0;
        
        switch (type)
        {
            case UpgradeType.DamagePerSecond:
                data = damagePerSecondUpgrade;
                currentLevel = damagePerSecondLevel;
                break;
            case UpgradeType.HitInterval:
                data = hitIntervalUpgrade;
                currentLevel = hitIntervalLevel;
                break;
            case UpgradeType.SawCount:
                data = sawCountUpgrade;
                currentLevel = sawCountLevel;
                break;
        }
        
        if (data == null || !CanAffordUpgrade(data, currentLevel))
            return false;
        
        int cost = data.GetCost(currentLevel);
        playerMoney -= cost;
        
        switch (type)
        {
            case UpgradeType.DamagePerSecond:
                damagePerSecondLevel++;
                break;
            case UpgradeType.HitInterval:
                hitIntervalLevel++;
                break;
            case UpgradeType.SawCount:
                sawCountLevel++;
                break;
        }
        
        SaveUpgrades();
        OnUpgradeChanged?.Invoke();
        OnMoneyChanged?.Invoke(playerMoney);
        
        return true;
    }
    
    public void AddMoney(int amount)
    {
        playerMoney += amount;
        OnMoneyChanged?.Invoke(playerMoney);
        SaveUpgrades();
    }
    
    private void SaveUpgrades()
    {
        PlayerPrefs.SetInt("DamagePerSecondLevel", damagePerSecondLevel);
        PlayerPrefs.SetInt("HitIntervalLevel", hitIntervalLevel);
        PlayerPrefs.SetInt("SawCountLevel", sawCountLevel);
        PlayerPrefs.SetInt("PlayerMoney", playerMoney);
        PlayerPrefs.Save();
    }
    
    private void LoadUpgrades()
    {
        damagePerSecondLevel = PlayerPrefs.GetInt("DamagePerSecondLevel", 0);
        hitIntervalLevel = PlayerPrefs.GetInt("HitIntervalLevel", 0);
        sawCountLevel = PlayerPrefs.GetInt("SawCountLevel", 0);
        playerMoney = PlayerPrefs.GetInt("PlayerMoney", 1000);
        
        OnMoneyChanged?.Invoke(playerMoney);
    }
    
    // Для тестування - скидання прогресу
    public void ResetUpgrades()
    {
        damagePerSecondLevel = 0;
        hitIntervalLevel = 0;
        sawCountLevel = 0;
        playerMoney = 1000;
        SaveUpgrades();
        OnUpgradeChanged?.Invoke();
        OnMoneyChanged?.Invoke(playerMoney);
    }
}

public enum UpgradeType
{
    DamagePerSecond,
    HitInterval,
    SawCount
}