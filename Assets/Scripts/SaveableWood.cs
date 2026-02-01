using System;
using UnityEngine;

public class SaveableWood : MonoBehaviour
{
    [SerializeField] private string woodId;

    public string WoodId => woodId;

    private void Awake()
    {
        if (string.IsNullOrEmpty(woodId))
        {
            woodId = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Встановити ID wood (для завантаження зі збереження)
    /// </summary>
    public void SetWoodId(string id)
    {
        woodId = id;
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(woodId))
        {
            woodId = Guid.NewGuid().ToString();
        }
    }
}