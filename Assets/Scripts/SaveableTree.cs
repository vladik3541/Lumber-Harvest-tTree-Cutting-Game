using UnityEngine;
using System;

/// <summary>
/// Компонент для дерев які можуть бути збережені
/// Додай на кожен префаб дерева
/// </summary>
public class SaveableTree : MonoBehaviour
{
    [SerializeField] private string treeId;

    public string TreeId => treeId;

    private void Awake()
    {
        // Генеруємо унікальний ID якщо його немає
        if (string.IsNullOrEmpty(treeId))
        {
            treeId = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Встановити ID дерева (для завантаження зі збереження)
    /// </summary>
    public void SetTreeId(string id)
    {
        treeId = id;
    }

    // Для Editor - генерування ID
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(treeId))
        {
            treeId = Guid.NewGuid().ToString();
        }
    }
}