using System;
using UnityEngine;

public class SellManager : MonoBehaviour
{
    [SerializeField] private Transform woodEndPosition;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out LogCollector logCollector))
        {
            logCollector.StartSellLog(woodEndPosition);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out LogCollector logCollector))
        {
            logCollector.StopFlyCoroutine();
        }
    }
}
