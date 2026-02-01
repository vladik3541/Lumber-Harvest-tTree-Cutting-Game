using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Головний клас збереження - містить всі дані гри
/// </summary>
[System.Serializable]
public class GameSaveData
{
    public PlayerSaveData playerData;
    public UpgradesSaveData upgradesData;
    public WorldSaveData worldData;
    public string saveDateTime;
    public int saveVersion = 1;

    public GameSaveData()
    {
        playerData = new PlayerSaveData();
        upgradesData = new UpgradesSaveData();
        worldData = new WorldSaveData();
        saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

/// <summary>
/// Дані гравця - позиція, обертання
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    public Vector3Data position;
    public Vector3Data rotation;
    public int money;

    public PlayerSaveData()
    {
        position = new Vector3Data(Vector3.zero);
        rotation = new Vector3Data(Vector3.zero);
        money = 0;
    }
}

/// <summary>
/// Дані прокачки
/// </summary>
[System.Serializable]
public class UpgradesSaveData
{
    public int damageLevel;
    public int intervalLevel;
    public int sawLevel;

    public UpgradesSaveData()
    {
        damageLevel = 0;
        intervalLevel = 0;
        sawLevel = 0;
    }
}

/// <summary>
/// Дані світу - дерева та wood об'єкти
/// </summary>
[System.Serializable]
public class WorldSaveData
{
    public List<TreeSaveData> trees;
    public List<WoodSaveData> woodObjects;

    public WorldSaveData()
    {
        trees = new List<TreeSaveData>();
        woodObjects = new List<WoodSaveData>();
    }
}

/// <summary>
/// Дані одного дерева
/// </summary>
[System.Serializable]
public class TreeSaveData
{
    public string treeId; // Унікальний ID
    public Vector3Data position;
    public Vector3Data rotation;
    public float currentHealth;
    public string treePrefabName; // Назва префабу для recreate

    public TreeSaveData(string id, Vector3 pos, Quaternion rot, float health, string prefabName)
    {
        treeId = id;
        position = new Vector3Data(pos);
        rotation = new Vector3Data(rot.eulerAngles);
        currentHealth = health;
        treePrefabName = prefabName;
    }
}

/// <summary>
/// Дані одного wood об'єкту
/// </summary>
[System.Serializable]
public class WoodSaveData
{
    public string woodId;
    public Vector3Data position;
    public Vector3Data rotation;
    public int cost;
    public bool isCollected;

    public WoodSaveData(string id, Vector3 pos, Quaternion rot, int woodCost, bool collected)
    {
        woodId = id;
        position = new Vector3Data(pos);
        rotation = new Vector3Data(rot.eulerAngles);
        cost = woodCost;
        isCollected = collected;
    }
}

/// <summary>
/// Серіалізований Vector3 (бо Unity's Vector3 не серіалізується в JSON)
/// </summary>
[System.Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3Data(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public static implicit operator Vector3(Vector3Data data)
    {
        return data.ToVector3();
    }
}
