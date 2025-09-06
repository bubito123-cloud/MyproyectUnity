using UnityEngine;

/// <summary>
/// Gestiona el movimiento f�sico del agente usando Rigidbody.MovePosition/MoveRotation.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 120f;
    public float maxVelocity = 5f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("MovementController requires a Rigidbody component!");
        }
    }

    public void ApplyMovement(float moveX, float moveZ, float rotate)
    {
        if (rb == null) return;

        // Clamp inputs
        moveX = Mathf.Clamp(moveX, -1f, 1f);
        moveZ = Mathf.Clamp(moveZ, -1f, 1f);
        rotate = Mathf.Clamp(rotate, -1f, 1f);

        // Apply rotation using physics
        float rotationAmount = rotate * rotationSpeed * Time.fixedDeltaTime;
        Quaternion deltaRotation = Quaternion.Euler(0f, rotationAmount, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);

        // Apply movement in local space
        Vector3 localMovement = new Vector3(moveX, 0f, moveZ);
        if (localMovement.sqrMagnitude > 1f)
            localMovement = localMovement.normalized;

        Vector3 worldMovement = transform.TransformDirection(localMovement);
        Vector3 newPosition = rb.position + worldMovement * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);

        // Limit velocity to prevent physics issues
        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
    }

    public Vector3 GetVelocity()
    {
        return rb != null ? rb.linearVelocity : Vector3.zero;
    }

    public void ResetPhysics()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}

