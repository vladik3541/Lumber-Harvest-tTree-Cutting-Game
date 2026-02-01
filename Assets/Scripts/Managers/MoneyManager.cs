using System;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private int moneyAmount;
    public event Action<int> OnChangeMoney;
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
    }

    private void Start()
    {
        OnChangeMoney?.Invoke(moneyAmount);
    }

    public int GetMoneyAmount()
    {
        return moneyAmount;
    }

    public void AddMoney(int amount)
    {
        moneyAmount += amount;
        OnChangeMoney?.Invoke(moneyAmount);
    }

    public void RemoveMoney(int amount)
    {
        moneyAmount -= amount;
        OnChangeMoney?.Invoke(moneyAmount);
    }

    public bool IsEnoughMoney(int amount)
    {
        return moneyAmount >= amount;
    }

    /// <summary>
    /// Встановлення грошей (для завантаження)
    /// </summary>
    public void SetMoney(int amount)
    {
        moneyAmount = amount;
        OnChangeMoney?.Invoke(moneyAmount);
    }
}