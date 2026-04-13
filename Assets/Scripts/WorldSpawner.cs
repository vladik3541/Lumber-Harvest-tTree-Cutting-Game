using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldSpawner : MonoBehaviour
{
    public static WorldSpawner Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject woodPrefab;

    [Header("Grid Settings")]
    [Tooltip("Кількість рядів дерев")]
    [SerializeField] private int rows = 5;

    [Tooltip("Кількість стовпців дерев")]
    [SerializeField] private int columns = 10;

    [Tooltip("Відстань між деревами по X")]
    [SerializeField] private float spacingX = 5f;

    [Tooltip("Відстань між деревами по Z")]
    [SerializeField] private float spacingZ = 5f;

    [Tooltip("Позиція першого дерева (лівий верхній кут сітки)")]
    [SerializeField] private Vector3 startPosition = Vector3.zero;

    [Tooltip("Випадковий поворот дерева по Y")]
    [SerializeField] private bool randomRotation = true;

    [Header("Spawn Sequence")]
    [Tooltip("Затримка між спавном кожного дерева (0 = миттєво)")]
    [SerializeField] private float spawnDelay = 0f;

    [Header("Parent Transforms")]
    [SerializeField] private Transform treesParent;
    [SerializeField] private Transform woodsParent;

    // Доступно з Editor'а
    public int TotalTrees => rows * columns;

    private Dictionary<string, GameObject> treePrefabDict;

    private void Awake()
    {
        Debug.Log("[WorldSpawner] Awake");
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogWarning("[WorldSpawner] Duplicate detected, destroying");
            Destroy(gameObject);
            return;
        }

        InitializePrefabDictionary();
        CreateParentTransforms();
        Debug.Log($"[WorldSpawner] Ready. Prefabs: {treePrefabs.Length}, Grid: {rows}x{columns}");
    }

    private void InitializePrefabDictionary()
    {
        treePrefabDict = new Dictionary<string, GameObject>();
        foreach (var prefab in treePrefabs)
        {
            if (prefab != null)
                treePrefabDict[prefab.name] = prefab;
        }
    }

    private void CreateParentTransforms()
    {
        if (treesParent == null)
            treesParent = new GameObject("Trees").transform;

        if (woodsParent == null)
            woodsParent = new GameObject("Woods").transform;
    }

    #region Initial Spawn   

    public void SpawnInitialWorld()
    {
        Debug.Log($"[WorldSpawner] SpawnInitialWorld called. Prefabs: {treePrefabs.Length}, Grid: {rows}x{columns}, Total: {TotalTrees}");
        if (treePrefabs.Length == 0)
        {
            Debug.LogError("[WorldSpawner] treePrefabs is EMPTY! Assign tree prefabs in Inspector.");
            return;
        }

        if (spawnDelay > 0f)
            StartCoroutine(SpawnSequence());
        else
            SpawnAllInstant();
    }

    private void SpawnAllInstant()
    {
        var positions = BuildGridPositions();
        Debug.Log($"[WorldSpawner] Spawning {positions.Count} trees instantly...");
        foreach (var pos in positions)
            SpawnRandomTree(pos);

        Debug.Log($"[WorldSpawner] Done. {positions.Count} trees spawned.");
    }

    private IEnumerator SpawnSequence()
    {
        var positions = BuildGridPositions();

        for (int i = 0; i < positions.Count; i++)
        {
            SpawnRandomTree(positions[i]);
            yield return new WaitForSeconds(spawnDelay);
        }

        Debug.Log($"Spawned {positions.Count} trees");
    }

    // Будує масив позицій по сітці: рядок за рядком, стовпець за стовпцем
    private List<Vector3> BuildGridPositions()
    {
        var positions = new List<Vector3>(rows * columns);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 pos = startPosition + new Vector3(col * spacingX, 0f, row * spacingZ);
                positions.Add(pos);
            }
        }

        return positions;
    }

    private void SpawnRandomTree(Vector3 position)
    {
        if (treePrefabs.Length == 0)
        {
            Debug.LogError("No tree prefabs assigned!");
            return;
        }

        var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
        var rotation = randomRotation
            ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
            : Quaternion.identity;

        SpawnTreeAtPosition(prefab, position, rotation);
    }

    #endregion

    #region Spawn From Save

    public void SpawnTree(TreeSaveData data)
    {
        var prefab = GetTreePrefabByName(data.treePrefabName);
        if (prefab == null)
        {
            Debug.LogWarning($"Tree prefab not found: {data.treePrefabName}");
            return;
        }

        var tree = SpawnTreeAtPosition(
            prefab,
            data.position.ToVector3(),
            Quaternion.Euler(data.rotation.ToVector3())
        );

        tree.GetComponent<TreeHealth>()?.SetHealth(data.currentHealth);
        tree.GetComponent<SaveableTree>()?.SetTreeId(data.treeId);
    }

    public void SpawnWood(WoodSaveData data)
    {
        if (woodPrefab == null)
        {
            Debug.LogError("Wood prefab not assigned!");
            return;
        }

        var woodObj = Instantiate(
            woodPrefab,
            data.position.ToVector3(),
            Quaternion.Euler(data.rotation.ToVector3()),
            woodsParent
        );

        var wood = woodObj.GetComponent<Wood>();
        if (wood != null)
        {
            wood.cost = data.cost;
            wood.isCollect = data.isCollected;
        }

        woodObj.GetComponent<SaveableWood>()?.SetWoodId(data.woodId);
    }

    #endregion

    #region Helpers

    private GameObject SpawnTreeAtPosition(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var tree = Instantiate(prefab, position, rotation, treesParent);

        if (tree.GetComponent<SaveableTree>() == null)
            tree.AddComponent<SaveableTree>();

        return tree;
    }

    private GameObject GetTreePrefabByName(string prefabName)
    {
        if (treePrefabDict.TryGetValue(prefabName, out var prefab))
            return prefab;

        foreach (var p in treePrefabs)
            if (p != null && p.name == prefabName)
                return p;

        return null;
    }

    #endregion

    #region Gizmos — preview grid in Scene view

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 pos = startPosition + new Vector3(col * spacingX, 0f, row * spacingZ);
                Gizmos.DrawSphere(pos, 0.4f);
            }
        }

        // Рамка навколо всієї сітки
        Vector3 gridStart = startPosition;
        Vector3 gridEnd = startPosition + new Vector3(
            (columns - 1) * spacingX,
            0f,
            (rows - 1) * spacingZ
        );
        Vector3 center = (gridStart + gridEnd) * 0.5f;
        Vector3 size = new Vector3(
            (columns - 1) * spacingX + spacingX,
            0.1f,
            (rows - 1) * spacingZ + spacingZ
        );

        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.2f);
        Gizmos.DrawCube(center, size);
    }

    #endregion
}
