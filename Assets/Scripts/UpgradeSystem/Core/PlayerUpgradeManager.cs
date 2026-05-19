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
    [SerializeField] private int damagePerSecondLevel = 1;
    [SerializeField] private int hitIntervalLevel = 1;
    [SerializeField] private int sawCountLevel = 1;

    public event Action OnUpgradeChanged;
    public event Action<int> OnMoneyChanged;

    public float CurrentDamagePerSecond => damagePerSecondUpgrade.GetValue(damagePerSecondLevel);
    public float CurrentHitInterval => hitIntervalUpgrade.GetValue(hitIntervalLevel);
    public GameObject CurrentSawPrefab => sawCountUpgrade.GetSawPrefab(sawCountLevel);

    public int DamagePerSecondLevel => damagePerSecondLevel;
    public int HitIntervalLevel => hitIntervalLevel;
    public int SawCountLevel => sawCountLevel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // НЕ завантажуємо тут - буде завантажено через SaveLoadManager
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
        return MoneyManager.Instance.IsEnoughMoney(cost);
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
        MoneyManager.Instance.RemoveMoney(cost);

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

        OnUpgradeChanged?.Invoke();
        OnMoneyChanged?.Invoke(MoneyManager.Instance.GetMoneyAmount());

        return true;
    }

    public void AddMoney(int amount)
    {
        MoneyManager.Instance.AddMoney(amount);
        OnMoneyChanged?.Invoke(MoneyManager.Instance.GetMoneyAmount());
    }

    /// <summary>
    /// Встановити гроші (для SaveLoadManager)
    /// </summary>
    public void SetMoney(int amount)
    {
        MoneyManager.Instance.SetMoney(amount);
        OnMoneyChanged?.Invoke(MoneyManager.Instance.GetMoneyAmount());
    }

    /// <summary>
    /// Завантаження апгрейдів зі збереження
    /// </summary>
    public void LoadUpgradesFromSave(int damage, int interval, int saw)
    {
        damagePerSecondLevel = Mathf.Max(1, damage);
        hitIntervalLevel = Mathf.Max(1, interval);
        sawCountLevel = saw;

        OnUpgradeChanged?.Invoke();

        Debug.Log($"✅ Апгрейди завантажено: Damage={damage}, Interval={interval}, Saw={saw}");
    }

    public void ResetUpgrades()
    {
        damagePerSecondLevel = 0;
        hitIntervalLevel = 0;
        sawCountLevel = 0;

        OnUpgradeChanged?.Invoke();
    }
}

public enum UpgradeType
{
    DamagePerSecond,
    HitInterval,
    SawCount
}