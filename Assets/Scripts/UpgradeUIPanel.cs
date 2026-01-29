using UnityEngine;
using TMPro;

public class UpgradeUIPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI moneyText;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private bool useAnimation = true;
    
    private bool isVisible = false;
    private PlayerUpgradeManager upgradeManager;
    
    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Початково панель прихована
        SetPanelVisibility(false, instant: true);
    }
    
    private void Start()
    {
        upgradeManager = PlayerUpgradeManager.Instance;
        
        if (upgradeManager != null)
        {
            upgradeManager.OnMoneyChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(upgradeManager.Money);
        }
    }
    
    private void OnDestroy()
    {
        if (upgradeManager != null)
        {
            upgradeManager.OnMoneyChanged -= UpdateMoneyDisplay;
        }
    }
    
    private void Update()
    {
        if (useAnimation && canvasGroup != null)
        {
            float targetAlpha = isVisible ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }
    
    public void ShowPanel()
    {
        SetPanelVisibility(true);
    }
    
    public void HidePanel()
    {
        SetPanelVisibility(false);
    }
    
    private void SetPanelVisibility(bool visible, bool instant = false)
    {
        isVisible = visible;
        
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
        
        if (canvasGroup != null)
        {
            if (instant || !useAnimation)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
            }
            
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }
    
    private void UpdateMoneyDisplay(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = $"${money}";
        }
    }
    
    // Метод для закриття панелі кнопкою (опціонально)
    public void OnCloseButtonClicked()
    {
        HidePanel();
    }
}