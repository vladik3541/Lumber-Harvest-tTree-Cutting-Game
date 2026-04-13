using UnityEngine;

public class TreeHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [SerializeField] private Wood wood;
    private Animator animator;

    // Публічна властивість для збереження
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Start()
    {
        animator = GetComponent<Animator>();

        // Ініціалізуємо здоров'я тільки якщо воно не було встановлене
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            OnTreeDestroyed();
        }
    }

    /// <summary>
    /// Встановлення здоров'я при завантаженні збереження
    /// </summary>
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
    }

    private void OnTreeDestroyed()
    {
        // Створюємо wood з SaveableWood компонентом
        GameObject woodInstance = Instantiate(wood.gameObject, transform.position, Quaternion.identity);

        // Додаємо SaveableWood якщо його немає
        if (woodInstance.GetComponent<SaveableWood>() == null)
        {
            woodInstance.AddComponent<SaveableWood>();
        }

        Destroy(gameObject);
    }
}