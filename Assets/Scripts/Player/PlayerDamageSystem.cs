using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

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
    private bool isInitialized = false;

    public void Initialize()
    {
        // Чекаємо поки SaveLoadManager завантажить дані
        StartCoroutine(InitializeAfterLoad());
    }

    private IEnumerator InitializeAfterLoad()
    {
        // Чекаємо один кадр щоб SaveLoadManager встиг завантажити дані
        yield return new WaitForEndOfFrame();

        upgradeManager = PlayerUpgradeManager.Instance;

        if (upgradeManager == null)
        {
            Debug.LogError("PlayerUpgradeManager не знайдено!");
            yield break;
        }

        // Підписуємось на зміни апгрейдів
        upgradeManager.OnUpgradeChanged += OnUpgradesChanged;

        // Створюємо початкову пилку
        UpdateSaw();

        isInitialized = true;

        Debug.Log($"✅ PlayerDamageSystem ініціалізовано. Урон: {upgradeManager.CurrentDamagePerSecond}");
    }

    private void OnDestroy()
    {
        if (upgradeManager != null)
        {
            upgradeManager.OnUpgradeChanged -= OnUpgradesChanged;
        }
    }

    private void OnUpgradesChanged()
    {
        // Коли змінюється тип пилки - оновлюємо візуал
        UpdateSaw();
    }

    private void UpdateSaw()
    {
        if (!isInitialized && upgradeManager == null)
        {
            Debug.LogWarning("UpdateSaw викликано до ініціалізації!");
            return;
        }

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

        // Ініціалізуємо всі Saw компоненти
        Saw[] saws = currentSawInstance.GetComponentsInChildren<Saw>();

        float damage = upgradeManager.CurrentDamagePerSecond;
        float hitInterval = upgradeManager.CurrentHitInterval;

        Debug.Log($"🔧 Ініціалізація пилки: Damage={damage}, Interval={hitInterval}");

        foreach (Saw saw in saws)
        {
            saw.Initialize(damage, 300f, hitInterval, sawPrefab);
        }

        Debug.Log($"✅ Пилка оновлена на: {sawPrefab.name}");
    }
}