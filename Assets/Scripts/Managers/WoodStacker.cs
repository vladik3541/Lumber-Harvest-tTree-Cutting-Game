using UnityEngine;
using DG.Tweening;
using System.Collections;

public class WoodStacker : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private Transform inventoryPosition; // Куди летять колоди
    [SerializeField] private float collectionRadius = 3f;
    [SerializeField] private LayerMask logLayer;
    
    [Header("Animation Settings")]
    [SerializeField] private float flyDuration = 0.8f;
    [SerializeField] private float selFlyDuration = 0.2f;
    [SerializeField] private float stackDelay = 0.15f;
    [SerializeField] private float arcHeight = 5f; // Висота дуги польоту
    
    [Header("Stack Positioning")]
    [SerializeField] private Vector3 stackOffset = new Vector3(0.3f, 0.15f, 0);
    [SerializeField] private int logsPerRow = 2;
    
    [Header("Visual Effects")]
    [SerializeField] private float scaleMultiplier = 0.3f;
    [SerializeField] private float punchScale = 0.15f;

    private Coroutine flyCoroutine;
    private bool isCollecting = false;

    public void Update()
    {
        if (!isCollecting)
            CollectNearbyLogs();
    }

    public void CollectNearbyLogs()
    {
        Collider[] logsInRange = Physics.OverlapSphere(transform.position, collectionRadius, logLayer);

        if (logsInRange.Length == 0)
            return;

        StartCoroutine(CollectLogsSequence(logsInRange));
    }

    private IEnumerator CollectLogsSequence(Collider[] logs)
    {
        isCollecting = true;

        foreach (Collider logCollider in logs)
        {
            if (Inventory.Instance.OnLimited())
            {
                isCollecting = false;
                yield break;
            }
                

            if (!logCollider.TryGetComponent(out Wood wood) || wood.isCollect) continue;

            GameObject log = logCollider.gameObject;
            
            log.transform.parent = inventoryPosition;
            wood.isCollect = true;
            
            // Додаємо в інвентар
            Inventory.Instance.AddWood(wood);
            
            AnimateLogCollection(log, Inventory.Instance.GetCount() - 1);
            
            yield return new WaitForSeconds(stackDelay);
        }

        isCollecting = false;
    }

    public void StartSellLog(Transform endPosition)
    {
        flyCoroutine = StartCoroutine(SellLogsSequence(endPosition));
    }

    public void StopFlyCoroutine()
    {
        if (flyCoroutine == null) return;
        StopCoroutine(flyCoroutine);
        flyCoroutine = null;
    }
    private IEnumerator SellLogsSequence(Transform endPosition)
    {
        while (Inventory.Instance.GetCount() > 0)
        {
            var wood = Inventory.Instance.RemoveWood().gameObject;
            MoneyManager.Instance.AddMoney(wood.GetComponent<Wood>().cost);
            AnimateLogSell(wood, endPosition.position);
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