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
    }
    
    public void UpdateStats(float damage, float speedSaw, float intervalDamage)
    {
        this.damage = damage;
        this.speedSaw = speedSaw;
        this.intervalDamage = intervalDamage;
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out TreeHealth health))
        {
            health.GetComponent<Animator>().SetBool("Hit", true);
            if (Time.time > timer)
            {
                health.TakeDamage(damage);
                Debug.Log("Damage saw " + damage);
                timer = Time.time + intervalDamage;
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out TreeHealth health))
        {
            health.GetComponent<Animator>().SetBool("Hit", false);
        }
    }
    private void Update()
    {
        transform.Rotate(0, Time.deltaTime * speedSaw, 0);
    }
}