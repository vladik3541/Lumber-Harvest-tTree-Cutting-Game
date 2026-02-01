using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject maxLevelIndicator;
    
    [Header("Settings")]
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = Color.gray;
    [SerializeField] private Color maxLevelColor = new Color(1f, 0.84f, 0f); // Золотий
    
    private UpgradeData upgradeData;
    private PlayerUpgradeManager upgradeManager;
    
    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        
        button.onClick.AddListener(OnUpgradeButtonClicked);
    }
    
    private void Start()
    {
        upgradeManager = PlayerUpgradeManager.Instance;
        
        if (upgradeManager == null)
        {
            Debug.LogError("PlayerUpgradeManager не знайдено!");
            return;
        }
        
        // Отримуємо відповідні дані апгрейду
        switch (upgradeType)
        {
            case UpgradeType.DamagePerSecond:
                upgradeData = upgradeManager.damagePerSecondUpgrade;
                break;
            case UpgradeType.HitInterval:
                upgradeData = upgradeManager.hitIntervalUpgrade;
                break;
            case UpgradeType.SawCount:
                upgradeData = upgradeManager.sawCountUpgrade;
                break;
        }
        
        if (upgradeData != null && icon != null)
        {
            icon.sprite = upgradeData.icon;
        }
        
        // Підписуємось на зміни
        upgradeManager.OnUpgradeChanged += UpdateUI;
        upgradeManager.OnMoneyChanged += OnMoneyChanged;
        
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        if (upgradeManager != null)
        {
            upgradeManager.OnUpgradeChanged -= UpdateUI;
            upgradeManager.OnMoneyChanged -= OnMoneyChanged;
        }
        
        button.onClick.RemoveListener(OnUpgradeButtonClicked);
    }
    
    private void UpdateUI()
    {
        if (upgradeData == null || upgradeManager == null)
            return;
        
        int currentLevel = GetCurrentLevel();
        bool isMaxLevel = currentLevel >= upgradeData.maxLevel;
        
        // Назва апгрейду
        if (titleText != null)
            titleText.text = upgradeData.upgradeName;
        
        // Рівень
        if (levelText != null)
        {
            if (isMaxLevel)
                levelText.text = $"МАКС";
            else
                levelText.text = $"Рівень {currentLevel + 1}";
        }
        
        // Поточне значення
        if (valueText != null)
        {
            if (upgradeData.isSawUpgrade)
            {
                // Для пилок - показуємо опис рівня
                string levelDesc = upgradeData.GetLevelDescription(currentLevel);
                valueText.text = !string.IsNullOrEmpty(levelDesc) ? levelDesc : "Пилка";
            }
            else
            {
                // Для звичайних апгрейдів
                float currentValue = upgradeData.GetValue(currentLevel);
                
                switch (upgradeType)
                {
                    case UpgradeType.DamagePerSecond:
                        valueText.text = $"Урон: {currentValue:F1}/сек";
                        break;
                    case UpgradeType.HitInterval:
                        valueText.text = $"Інтервал: {currentValue:F2}сек";
                        break;
                    case UpgradeType.SawCount:
                        valueText.text = $"Пилок: {currentValue:F0}";
                        break;
                }
            }
        }
        
        // Вартість наступного рівня
        if (costText != null)
        {
            if (isMaxLevel)
            {
                costText.text = "MAX";
            }
            else
            {
                int cost = upgradeData.GetCost(currentLevel);
                costText.text = $"${cost}";
            }
        }
        
        // Показуємо/ховаємо індикатор максимального рівня
        if (maxLevelIndicator != null)
            maxLevelIndicator.SetActive(isMaxLevel);
        
        // Оновлюємо стан кнопки
        UpdateButtonState(isMaxLevel);
    }
    
    private void UpdateButtonState(bool isMaxLevel)
    {
        if (isMaxLevel)
        {
            button.interactable = false;
            if (costText != null)
                costText.color = maxLevelColor;
        }
        else
        {
            int currentLevel = GetCurrentLevel();
            bool canAfford = upgradeManager.CanAffordUpgrade(upgradeData, currentLevel);
            
            button.interactable = canAfford;
            
            if (costText != null)
                costText.color = canAfford ? affordableColor : unaffordableColor;
        }
    }
    
    private void OnMoneyChanged(int newMoney)
    {
        UpdateUI();
    }
    
    private void OnUpgradeButtonClicked()
    {
        if (upgradeManager.TryUpgrade(upgradeType))
        {
            // Можна додати звук або ефект
            Debug.Log($"Прокачано {upgradeType}!");
        }
        else
        {
            Debug.Log($"Не вдалось прокачати {upgradeType}. Недостатньо коштів або досягнуто максимальний рівень.");
        }
    }
    
    private int GetCurrentLevel()
    {
        switch (upgradeType)
        {
            case UpgradeType.DamagePerSecond:
                return upgradeManager.DamagePerSecondLevel;
            case UpgradeType.HitInterval:
                return upgradeManager.HitIntervalLevel;
            case UpgradeType.SawCount:
                return upgradeManager.SawCountLevel;
            default:
                return 0;
        }
    }
}