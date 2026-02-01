using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UpgradeZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UpgradeUIPanel upgradePanel;
    
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool showDebugMessages = true;
    
    private bool isPlayerInZone = false;
    
    private void Awake()
    {
        // Переконуємось що колайдер є тригером
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"Upgrade Zone на {gameObject.name} не був тригером. Автоматично виправлено.");
        }
    }
    
    private void Start()
    {
        if (upgradePanel == null)
        {
            upgradePanel = FindObjectOfType<UpgradeUIPanel>();
            if (upgradePanel == null)
            {
                Debug.LogError("UpgradeUIPanel не знайдено! Додай UpgradeUIPanel на сцену.");
            }
        }
        
        // Переконуємось що панель прихована на старті
        if (upgradePanel != null)
        {
            upgradePanel.HidePanel();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInZone = true;
            
            if (upgradePanel != null)
            {
                upgradePanel.ShowPanel();
                
                if (showDebugMessages)
                    Debug.Log("Гравець увійшов в зону апгрейдів");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInZone = false;
            
            if (upgradePanel != null)
            {
                upgradePanel.HidePanel();
                
                if (showDebugMessages)
                    Debug.Log("Гравець вийшов з зони апгрейдів");
            }
        }
    }
    
    // Візуалізація зони в редакторі
    private void OnDrawGizmos()
    {
        Gizmos.color = isPlayerInZone ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}