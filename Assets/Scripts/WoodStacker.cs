using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class LogCollector : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private Transform inventoryPosition; // Куди летять колоди
    [SerializeField] private float collectionRadius = 3f;
    [SerializeField] private LayerMask logLayer;
    
    [Header("Animation Settings")]
    [SerializeField] private float flyDuration = 0.8f;
    [SerializeField] private float selFlyDuration = 0.8f;
    [SerializeField] private float stackDelay = 0.15f;
    [SerializeField] private float arcHeight = 2f; // Висота дуги польоту
    
    [Header("Stack Positioning")]
    [SerializeField] private Vector3 stackOffset = new Vector3(0.3f, 0.15f, 0);
    [SerializeField] private int logsPerRow = 3;
    
    [Header("Visual Effects")]
    [SerializeField] private float scaleMultiplier = 0.3f;
    [SerializeField] private float punchScale = 0.15f;

    private Coroutine flyCoroutine;
    public void Update()
    {
        CollectNearbyLogs();
    }

    public void CollectNearbyLogs()
    {
        Collider[] logsInRange = Physics.OverlapSphere(transform.position, collectionRadius, logLayer);
        
        if (logsInRange.Length == 0)
        {
            Debug.Log("Немає колод поблизу!");
            return;
        }

        StartCoroutine(CollectLogsSequence(logsInRange));
    }

    private IEnumerator CollectLogsSequence(Collider[] logs)
    {
        foreach (Collider logCollider in logs)
        {
            if (Inventory.Instance.OnLimited())
            {
                Debug.Log("Інвентар повний!");
                yield break;
            }

            if (logCollider.gameObject.GetComponent<Wood>().isCollect) continue;

            GameObject log = logCollider.gameObject;
            
            // Встановлюємо parent та виключаємо колізію ДО анімації
            log.transform.parent = inventoryPosition;
            Wood wood = log.GetComponent<Wood>();
            wood.isCollect = true;
            
            // Додаємо в інвентар
            Inventory.Instance.AddWood(wood);
            
            // Запускаємо анімацію
            AnimateLogCollection(log, Inventory.Instance.GetCount() - 1);
            
            yield return new WaitForSeconds(stackDelay);
        }
    }

    public void StartSellLog(Transform endPosition)
    {
        flyCoroutine = StartCoroutine(SellLogsSequence(endPosition));
    }

    public void StopFlyCoroutine()
    {
        StopCoroutine(flyCoroutine);
    }
    private IEnumerator SellLogsSequence(Transform endPosition)
    {
        while (Inventory.Instance.GetCount() > 0)
        {
            AnimateLogSell(Inventory.Instance.RemoveWood().gameObject, endPosition.position);
            yield return new WaitForSeconds(0.05f);
        }
        
    }
    private void AnimateLogCollection(GameObject log, int stackIndex)
    {
        Vector3 endPosition = GetStackPosition(stackIndex);
       log.transform.DOLocalJump(endPosition, arcHeight, 1, flyDuration).OnComplete(()=>log.transform.localPosition = endPosition);
       log.transform.DOLocalRotate(new Vector3(90, 0, 0f), flyDuration);
    }
    private void AnimateLogSell(GameObject log, Vector3 endPosition)
    {
        log.transform.parent = null;
        log.transform.DOJump(endPosition, arcHeight, 1, selFlyDuration).OnComplete(()=>Destroy(log.gameObject));
        log.transform.DOLocalRotate(new Vector3(90, 0, 0f), flyDuration);
    }
    
    private Vector3 GetStackPosition(int index)
    {
        int row = index / logsPerRow;
        int column = index % logsPerRow;

        float xOffset = (column - (logsPerRow - 1) / 2f) * stackOffset.x;
        float yOffset = row * stackOffset.y;
        float zOffset = row * stackOffset.z;

        return new Vector3(xOffset, yOffset, zOffset);
    }
    
}