using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}
