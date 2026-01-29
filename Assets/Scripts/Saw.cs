using UnityEngine;

public class Saw : MonoBehaviour
{
    private float damage;
    private float speedSaw;
    private float intervalDamage;
    private float timer = 0f;
    
    private GameObject currentSawModel;

    public void Initialize(float damage, float speedSaw, float intervalDamage, GameObject sawPrefab)
    {
        this.damage = damage;
        this.speedSaw = speedSaw;
        this.intervalDamage = intervalDamage;
        
        UpdateSawModel(sawPrefab);
    }
    
    public void UpdateStats(float damage, float speedSaw, float intervalDamage)
    {
        this.damage = damage;
        this.speedSaw = speedSaw;
        this.intervalDamage = intervalDamage;
    }
    
    public void UpdateSawModel(GameObject sawPrefab)
    {
        // Видалити стару модель
        if (currentSawModel != null)
        {
            Destroy(currentSawModel);
        }
        
        // Створити нову модель
        if (sawPrefab != null)
        {
            currentSawModel = Instantiate(sawPrefab, transform);
            currentSawModel.transform.localPosition = Vector3.zero;
            currentSawModel.transform.localRotation = Quaternion.identity;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out TreeHealth health))
        {
            if (Time.time > timer)
            {
                health.TakeDamage(damage);
                timer = Time.time + intervalDamage;
            }
        }
    }

    private void Update()
    {
        transform.Rotate(0, Time.deltaTime * speedSaw, 0);
    }
}