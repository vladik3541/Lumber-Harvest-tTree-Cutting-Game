using System;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private int moneyAmount;

    private void Start()
    {
        moneyText.text = moneyAmount.ToString();
    }

    public int GetMoneyAmount()
    {
        return moneyAmount;
    }

    public void AddMoney(int amount)
    {
        moneyAmount += amount;
        moneyText.text = moneyAmount.ToString();
    }

    public void RemoveMoney(int amount)
    {
        moneyAmount -= amount;
        moneyText.text = moneyAmount.ToString();
    }

    public bool IsEnoughMoney(int amount)
    {
        return moneyAmount >= amount;
    }
}
