using UnityEngine;

public class TreeHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [SerializeField] private Wood wood;
    private Animator animator;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("Hit");
        // Візуальний фідбек
        Debug.Log($"{gameObject.name} отримало {damage} шкоди. Залишилось: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            OnTreeDestroyed();
        }
    }
    
    private void OnTreeDestroyed()
    {
        Instantiate(wood, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
    }
}
