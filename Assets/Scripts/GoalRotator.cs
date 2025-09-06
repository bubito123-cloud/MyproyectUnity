using UnityEngine;

// Helper component for goal animation
public class GoalRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationSpeed = new Vector3(0, 45, 0);
    public bool enablePulsing = true;
    public float pulseSpeed = 2f;
    public float pulseScale = 0.2f;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Rotate goal
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // Pulse scale
        if (enablePulsing)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = originalScale + Vector3.one * pulse;
        }
    }
}
