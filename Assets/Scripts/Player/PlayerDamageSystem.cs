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

    [Header("Visual Settings")]
    [SerializeField] private bool showDebugGizmos = true;

    private PlayerUpgradeManager upgradeManager;
    private float damageTimer = 0f;
    private GameObject currentSawInstance;

    public void Initialize()
    {
        StartCoroutine(InitializeAfterLoad());
    }

    private IEnumerator InitializeAfterLoad()
    {
        // Чекаємо поки PlayerUpgradeManager стане доступним
        yield return new WaitUntil(() => PlayerUpgradeManager.Instance != null);

        upgradeManager = PlayerUpgradeManager.Instance;
        upgradeManager.OnUpgradeChanged += OnUpgradesChanged;

        UpdateSaw();

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
        if (upgradeManager == null)
            return;

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
            saw.Initialize(damage, 300f, hitInterval);
        }

        Debug.Log($"✅ Пилка оновлена на: {sawPrefab.name}");
    }
}