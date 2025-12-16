using UnityEngine;

public class TornadoSpin : MonoBehaviour
{
    [Header("Tornado Spin Settings")]
    [SerializeField] private float rotationSpeed = 180f;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
