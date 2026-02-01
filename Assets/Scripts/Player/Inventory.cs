using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    [SerializeField] private int maxLog;
    [SerializeField] private Gradient colorFill;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private Image fillImage;
    private Stack<Wood> inventory = new Stack<Wood>();
    public event Action<int> OnInventoryChanged;

    private void Awake()
    {
        Instance = this;
        SetColorAndText();
    }

    public void AddWood(Wood amount)
    {
        inventory.Push(amount);
        OnInventoryChanged?.Invoke(inventory.Count);
        SetColorAndText();
    }
    public Wood RemoveWood()
    {
        Wood item = inventory.Pop();
        OnInventoryChanged?.Invoke(inventory.Count);
        SetColorAndText();
        return item;
    }
    
    public int GetCount()
    {
        return inventory.Count;
    }

    public bool OnLimited()
    {
        return inventory.Count >= maxLog;
    }

    private void SetColorAndText()
    {
        float progress = (float)inventory.Count / maxLog;

        fillImage.fillAmount = progress;
        fillImage.color = colorFill.Evaluate((float)inventory.Count / maxLog);
        logText.text = GetCount() + " / " + maxLog;
    }
}
