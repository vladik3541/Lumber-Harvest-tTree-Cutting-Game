using UnityEngine;

public class SawRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    
    private void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
