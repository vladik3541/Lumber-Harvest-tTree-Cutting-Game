using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController3D : MonoBehaviour
{
    [Header("Налаштування руху")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    private Vector2 moveInput;
    private Rigidbody rb;
    private InputActionSystem inputActionSystem;
    private InputAction moveAction;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Заморожуємо обертання по осях X і Z, щоб персонаж не падав
        rb.freezeRotation = true;
        inputActionSystem = new InputActionSystem();
        moveAction = inputActionSystem.Player.Move;
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        inputActionSystem.Enable();
    }
    
    // Цей метод викликається новою системою вводу
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    private void FixedUpdate()
    {
        MoveCharacter();
        RotateCharacter();
    }
    
    private void MoveCharacter()
    {
        // Створюємо вектор руху в 3D просторі
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        
        // Переміщуємо персонажа
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
    
    private void RotateCharacter()
    {
        // Якщо персонаж рухається
        if (moveInput.sqrMagnitude > 0.01f)
        {
            // Створюємо напрямок руху
            Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
            
            // Створюємо обертання в бік руху
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Плавно обертаємо персонажа
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}