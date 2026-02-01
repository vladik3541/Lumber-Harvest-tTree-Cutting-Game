using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Спавнер світу - відповідає за створення дерев та wood об'єктів
/// </summary>
public class WorldSpawner : MonoBehaviour
{
    public static WorldSpawner Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject[] treePrefabs; // Масив префабів дерев
    [SerializeField] private GameObject woodPrefab;

    [Header("Initial Spawn (New Game)")]
    [SerializeField] private bool spawnTreesOnNewGame = true;
    [SerializeField] private int initialTreeCount = 50;
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-50, -50);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(50, 50);
    [SerializeField] private float minTreeDistance = 5f;

    [Header("Parent Transforms")]
    [SerializeField] private Transform treesParent;
    [SerializeField] private Transform woodsParent;

    private Dictionary<string, GameObject> treePrefabDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializePrefabDictionary();
        CreateParentTransforms();
    }

    private void InitializePrefabDictionary()
    {
        treePrefabDict = new Dictionary<string, GameObject>();
        foreach (GameObject prefab in treePrefabs)
        {
            if (prefab != null)
            {
                treePrefabDict[prefab.name] = prefab;
            }
        }
    }

    private void CreateParentTransforms()
    {
        if (treesParent == null)
        {
            treesParent = new GameObject("Trees").transform;
        }

        if (woodsParent == null)
        {
            woodsParent = new GameObject("Woods").transform;
        }
    }

    #region Initial World

    public void SpawnInitialWorld()
    {
        if (!spawnTreesOnNewGame)
            return;

        Debug.Log($"🌲 Генерація початкового світу: {initialTreeCount} дерев");

        List<Vector3> spawnedPositions = new List<Vector3>();

        for (int i = 0; i < initialTreeCount; i++)
        {
            Vector3 position = GetRandomSpawnPosition(spawnedPositions);
            GameObject treePrefab = GetRandomTreePrefab();

            if (treePrefab != null)
            {
                SpawnTreeAtPosition(treePrefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                spawnedPositions.Add(position);
            }
        }

        Debug.Log($"✅ Світ згенеровано: {spawnedPositions.Count} дерев");
    }

    private Vector3 GetRandomSpawnPosition(List<Vector3> existingPositions)
    {
        const int maxAttempts = 30;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float z = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            Vector3 position = new Vector3(x, 0, z);

            // Перевіряємо чи не занадто близько до інших дерев
            bool tooClose = false;
            foreach (Vector3 existingPos in existingPositions)
            {
                if (Vector3.Distance(position, existingPos) < minTreeDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                // Raycast вниз щоб знайти землю
                if (Physics.Raycast(position + Vector3.up * 100, Vector3.down, out RaycastHit hit, 200f))
                {
                    return hit.point;
                }
                return position;
            }
        }

        // Якщо не знайшли - повертаємо випадкову позицію
        return new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            0,
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );
    }

    private GameObject GetRandomTreePrefab()
    {
        if (treePrefabs.Length == 0)
        {
            Debug.LogError("Немає префабів дерев!");
            return null;
        }

        return treePrefabs[Random.Range(0, treePrefabs.Length)];
    }

    #endregion

    #region Spawn From Save

    public void SpawnTree(TreeSaveData data)
    {
        GameObject prefab = GetTreePrefabByName(data.treePrefabName);
        
        if (prefab == null)
        {
            Debug.LogWarning($"Не знайдено префаб дерева: {data.treePrefabName}");
            return;
        }

        GameObject tree = SpawnTreeAtPosition(
            prefab, 
            data.position.ToVector3(), 
            Quaternion.Euler(data.rotation.ToVector3())
        );

        // Відновлюємо здоров'я
        TreeHealth health = tree.GetComponent<TreeHealth>();
        if (health != null)
        {
            health.SetHealth(data.currentHealth);
        }

        // Встановлюємо ID
        SaveableTree saveable = tree.GetComponent<SaveableTree>();
        if (saveable != null)
        {
            saveable.SetTreeId(data.treeId);
        }
    }

    public void SpawnWood(WoodSaveData data)
    {
        if (woodPrefab == null)
        {
            Debug.LogError("Wood prefab не встановлено!");
            return;
        }

        GameObject woodObj = Instantiate(
            woodPrefab,
            data.position.ToVector3(),
            Quaternion.Euler(data.rotation.ToVector3()),
            woodsParent
        );

        Wood wood = woodObj.GetComponent<Wood>();
        if (wood != null)
        {
            wood.cost = data.cost;
            wood.isCollect = data.isCollected;
        }

        SaveableWood saveable = woodObj.GetComponent<SaveableWood>();
        if (saveable != null)
        {
            saveable.SetWoodId(data.woodId);
        }
    }

    #endregion

    #region Helper Methods

    private GameObject SpawnTreeAtPosition(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject tree = Instantiate(prefab, position, rotation, treesParent);

        // Додаємо SaveableTree якщо його немає
        if (tree.GetComponent<SaveableTree>() == null)
        {
            tree.AddComponent<SaveableTree>();
        }

        return tree;
    }

    private GameObject GetTreePrefabByName(string prefabName)
    {
        if (treePrefabDict.TryGetValue(prefabName, out GameObject prefab))
        {
            return prefab;
        }

        // Пробуємо знайти в масиві напряму
        foreach (GameObject treePrefab in treePrefabs)
        {
            if (treePrefab != null && treePrefab.name == prefabName)
            {
                return treePrefab;
            }
        }

        return null;
    }

    #endregion
}
