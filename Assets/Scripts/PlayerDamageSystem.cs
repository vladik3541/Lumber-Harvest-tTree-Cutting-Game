using UnityEngine;
using UnityEngine.Serialization;

public class PlayerDamageSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform sawSpawnPoint;
    
    [FormerlySerializedAs("detectionRadius")]
    [Header("Detection Settings")]
    [SerializeField] private float interval = 3f;
    [SerializeField] private LayerMask treeLayer;
    
    [Header("Visual Settings")]
    [SerializeField] private bool showDebugGizmos = true;
    
    private PlayerUpgradeManager upgradeManager;
    private float damageTimer = 0f;
    private GameObject currentSawInstance;
    
    private void Start()
    {
        upgradeManager = PlayerUpgradeManager.Instance;
        
        if (upgradeManager == null)
        {
            Debug.LogError("PlayerUpgradeManager не знайдено!");
            return;
        }
        
        // Підписуємось на зміни апгрейдів
        upgradeManager.OnUpgradeChanged += OnUpgradesChanged;
        
        // Створюємо початкову пилку
        UpdateSaw();
    }
    
    private void OnDestroy()
    {
        if (upgradeManager != null)
        {
            upgradeManager.OnUpgradeChanged -= OnUpgradesChanged;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out TreeHealth heath))
        {
            if (Time.time >= damageTimer)
            {
                damageTimer = Time.time + interval;
            }
            heath.GetComponent<Animator>().SetBool("Hit", true);
            float damagePerHit = upgradeManager.CurrentDamagePerSecond;
            heath.TakeDamage(damagePerHit);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out TreeHealth heath))
        {
            heath.GetComponent<Animator>().SetBool("Hit", false);
        }
    }

    private void OnUpgradesChanged()
    {
        // Коли змінюється тип пилки - оновлюємо візуал
        UpdateSaw();
    }
    
    private void UpdateSaw()
    {
        // Видаляємо стару пилку
        if (currentSawInstance != null)
        {
            Destroy(currentSawInstance);
            currentSawInstance = null;
        }
        
        GameObject sawPrefab = upgradeManager.CurrentSawPrefab;
        
        if (sawPrefab == null)
        {
            Debug.LogWarning("Saw prefab не встановлено для поточного рівня!");
            return;
        }
        
        if (sawSpawnPoint == null)
        {
            Debug.LogError("Saw Spawn Point не встановлено!");
            return;
        }
        
        // Створюємо нову пилку
        currentSawInstance = Instantiate(sawPrefab, sawSpawnPoint);
        currentSawInstance.transform.localPosition = Vector3.zero;
        currentSawInstance.transform.localRotation = Quaternion.identity;
        
        // Додаємо обертання якщо його немає
        if (currentSawInstance.GetComponent<SawRotation>() == null)
        {
            currentSawInstance.AddComponent<SawRotation>();
        }
        
        Debug.Log($"Пилка оновлена на: {sawPrefab.name}");
    }
}
