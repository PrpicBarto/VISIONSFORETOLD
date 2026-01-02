using UnityEngine;


public class FootstepVFX : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float stepInterval = 0.5f; // Time between steps
    [SerializeField] private float minimumSpeed = 0.5f; // Minimum speed to trigger
    [SerializeField] private Vector3 effectOffset = new Vector3(0, 0.1f, 0);
    
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    
    private float stepTimer;
    private Vector3 lastPosition;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        
        lastPosition = transform.position;
    }

    private void Update()
    {
        // Check if moving
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;
        
        if (speed >= minimumSpeed)
        {
            stepTimer += Time.deltaTime;
            
            if (stepTimer >= stepInterval)
            {
                SpawnFootstepEffect();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void SpawnFootstepEffect()
    {
        if (VFXManager.Instance != null)
        {
            Vector3 effectPos = transform.position + effectOffset;
            VFXManager.Instance.PlayWalkDust(effectPos);
        }
    }
}
