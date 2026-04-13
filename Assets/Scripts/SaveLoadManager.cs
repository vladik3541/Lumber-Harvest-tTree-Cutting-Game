using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Менеджер збереження/завантаження
/// Singleton патерн для глобального доступу
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool useEncryption = false;
    [SerializeField] private bool prettyPrint = true;
    [SerializeField] private string saveFileName = "gamesave.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public event Action OnBeforeSave;
    public event Action OnAfterSave;
    public event Action OnBeforeLoad;
    public event Action<GameSaveData> OnAfterLoad;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Зберегти гру
    /// </summary>
    public void SaveGame()
    {
        try
        {
            OnBeforeSave?.Invoke();

            GameSaveData saveData = new GameSaveData();

            // Збираємо дані з усіх систем
            CollectPlayerData(saveData);
            CollectUpgradesData(saveData);
            CollectWorldData(saveData);

            // Серіалізуємо в JSON
            string json = JsonUtility.ToJson(saveData, prettyPrint);

            if (useEncryption)
            {
                json = EncryptDecrypt(json);
            }

            // Зберігаємо файл
            File.WriteAllText(SavePath, json);

            OnAfterSave?.Invoke();

            Debug.Log($"✅ Гру збережено: {SavePath}");
            Debug.Log($"📊 Розмір: {new FileInfo(SavePath).Length / 1024f:F2} KB");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Помилка збереження: {e.Message}");
        }
    }

    /// <summary>
    /// Завантажити гру
    /// </summary>
    public bool LoadGame()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("⚠️ Файл збереження не знайдено");
            return false;
        }

        try
        {
            OnBeforeLoad?.Invoke();

            // Читаємо файл
            string json = File.ReadAllText(SavePath);

            if (useEncryption)
            {
                json = EncryptDecrypt(json);
            }

            // Десеріалізуємо
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

            if (saveData == null)
            {
                Debug.LogError("❌ Не вдалось десеріалізувати дані");
                return false;
            }

            // Застосовуємо дані
            ApplyPlayerData(saveData);
            ApplyUpgradesData(saveData);
            ApplyWorldData(saveData);

            OnAfterLoad?.Invoke(saveData);

            Debug.Log($"✅ Гру завантажено: {saveData.saveDateTime}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Помилка завантаження: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Видалити збереження
    /// </summary>
    public void DeleteSave()
    {
        if (HasSaveFile())
        {
            File.Delete(SavePath);
            Debug.Log("🗑️ Збереження видалено");
        }
    }

    /// <summary>
    /// Чи є файл збереження
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    /// <summary>
    /// Отримати інформацію про збереження
    /// </summary>
    public SaveInfo GetSaveInfo()
    {
        if (!HasSaveFile())
            return null;

        try
        {
            string json = File.ReadAllText(SavePath);
            if (useEncryption)
                json = EncryptDecrypt(json);

            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            FileInfo fileInfo = new FileInfo(SavePath);

            return new SaveInfo
            {
                saveDateTime = data.saveDateTime,
                fileSize = fileInfo.Length,
                playerPosition = data.playerData.position.ToVector3(),
                money = data.playerData.money,
                treeCount = data.worldData.trees.Count,
                woodCount = data.worldData.woodObjects.Count
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"Помилка читання інфо: {e.Message}");
            return null;
        }
    }

    #region Data Collection

    private void CollectPlayerData(GameSaveData saveData)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            saveData.playerData.position = new Vector3Data(player.transform.position);
            saveData.playerData.rotation = new Vector3Data(player.transform.eulerAngles);
        }

        // Гроші з MoneyManager або PlayerUpgradeManager
        if (PlayerUpgradeManager.Instance != null)
        {
            saveData.playerData.money = MoneyManager.Instance.GetMoneyAmount();
        }
        else if (MoneyManager.Instance != null)
        {
            saveData.playerData.money = MoneyManager.Instance.GetMoneyAmount();
        }
    }

    private void CollectUpgradesData(GameSaveData saveData)
    {
        if (PlayerUpgradeManager.Instance != null)
        {
            saveData.upgradesData.damageLevel = PlayerUpgradeManager.Instance.DamagePerSecondLevel;
            saveData.upgradesData.intervalLevel = PlayerUpgradeManager.Instance.HitIntervalLevel;
            saveData.upgradesData.sawLevel = PlayerUpgradeManager.Instance.SawCountLevel;
        }
    }

    private void CollectWorldData(GameSaveData saveData)
    {
        // Збираємо дані всіх дерев
        TreeHealth[] trees = FindObjectsOfType<TreeHealth>();
        foreach (TreeHealth tree in trees)
        {
            if (tree.GetComponent<SaveableTree>() != null)
            {
                SaveableTree saveableTree = tree.GetComponent<SaveableTree>();
                TreeSaveData treeData = new TreeSaveData(
                    saveableTree.TreeId,
                    tree.transform.position,
                    tree.transform.rotation,
                    tree.CurrentHealth,
                    tree.gameObject.name.Replace("(Clone)", "").Trim()
                );
                saveData.worldData.trees.Add(treeData);
            }
        }

        // Збираємо дані всіх wood об'єктів
        Wood[] woods = FindObjectsOfType<Wood>();
        foreach (Wood wood in woods)
        {
            if (wood.GetComponent<SaveableWood>() != null)
            {
                SaveableWood saveableWood = wood.GetComponent<SaveableWood>();
                WoodSaveData woodData = new WoodSaveData(
                    saveableWood.WoodId,
                    wood.transform.position,
                    wood.transform.rotation,
                    wood.cost,
                    wood.isCollect
                );
                saveData.worldData.woodObjects.Add(woodData);
            }
        }

        Debug.Log($"📦 Зібрано: {saveData.worldData.trees.Count} дерев, {saveData.worldData.woodObjects.Count} wood");
    }

    #endregion

    #region Data Application

    private void ApplyPlayerData(GameSaveData saveData)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = saveData.playerData.position.ToVector3();
            player.transform.eulerAngles = saveData.playerData.rotation.ToVector3();
        }

        // Застосовуємо гроші
        if (PlayerUpgradeManager.Instance != null)
        {
            // Встановлюємо гроші через рефлексію або публічний метод
            PlayerUpgradeManager.Instance.SetMoney(saveData.playerData.money);
        }
    }

    private void ApplyUpgradesData(GameSaveData saveData)
    {
        if (PlayerUpgradeManager.Instance != null)
        {
            PlayerUpgradeManager.Instance.LoadUpgradesFromSave(
                saveData.upgradesData.damageLevel,
                saveData.upgradesData.intervalLevel,
                saveData.upgradesData.sawLevel
            );
        }
    }

    private void ApplyWorldData(GameSaveData saveData)
    {
        int treeCount = saveData.worldData.trees.Count;
        Debug.Log($"[SaveLoadManager] ApplyWorldData: {treeCount} trees in save");

        // Якщо збереження порожнє — спавнимо початковий світ
        if (treeCount == 0)
        {
            Debug.LogWarning("[SaveLoadManager] Save has 0 trees -> SpawnInitialWorld()");
            WorldSpawner.Instance?.SpawnInitialWorld();
            return;
        }

        ClearExistingWorldObjects();

        foreach (TreeSaveData treeData in saveData.worldData.trees)
            WorldSpawner.Instance?.SpawnTree(treeData);

        foreach (WoodSaveData woodData in saveData.worldData.woodObjects)
        {
            if (!woodData.isCollected)
                WorldSpawner.Instance?.SpawnWood(woodData);
        }
    }

    private void ClearExistingWorldObjects()
    {
        // Видаляємо всі дерева
        TreeHealth[] trees = FindObjectsOfType<TreeHealth>();
        foreach (TreeHealth tree in trees)
        {
            Destroy(tree.gameObject);
        }

        // Видаляємо всі wood об'єкти
        Wood[] woods = FindObjectsOfType<Wood>();
        foreach (Wood wood in woods)
        {
            Destroy(wood.gameObject);
        }
    }

    #endregion

    #region Encryption (Simple XOR)

    private string EncryptDecrypt(string text)
    {
        string key = "MySecretKey123"; // Змінити на свій
        char[] result = new char[text.Length];

        for (int i = 0; i < text.Length; i++)
        {
            result[i] = (char)(text[i] ^ key[i % key.Length]);
        }

        return new string(result);
    }

    #endregion
}

/// <summary>
/// Інформація про збереження для UI
/// </summary>
public class SaveInfo
{
    public string saveDateTime;
    public long fileSize;
    public Vector3 playerPosition;
    public int money;
    public int treeCount;
    public int woodCount;
}
