using UnityEngine;

/// <summary>
/// Represents a source of food that agents can interact with to gain satisfaction.
/// This object has a limited number of uses and a cooldown period.
/// </summary>
public class FoodSource : InteractableObject
{
    // --- FIX: Variables declared here --- 
    public string objectName;
    public bool isAvailable = true;
    // --- End of Fix ---

    [Header("Food Source Settings")]
    [Tooltip("The amount of satisfaction an agent gains from this food.")]
    public float satisfactionGain = 25f;

    [Tooltip("The maximum number of times this food source can be used.")]
    public int maxUses = 3;
    private int remainingUses;

    [Tooltip("The time in seconds it takes for this food source to become available again after being used.")]
    public float cooldownDuration = 10f;

    private void Start()
    {
        objectName = "Food Source";
        remainingUses = maxUses;
    }

    /// <summary>
    /// When an agent interacts with the food source, it gains satisfaction.
    /// The food source then goes on cooldown and loses one of its uses.
    /// </summary>
    public override void OnInteract(CognitiveController agentController)
    {
        if (isAvailable && remainingUses > 0)
        {
            isAvailable = false;
            remainingUses--;

            // Trigger a positive emotional event in the agent
            if (agentController.TryGetComponent<EmotionalCore>(out var emotionalCore))
            {
                emotionalCore.TriggerEmotionalEvent("satisfaction", satisfactionGain);
            }
            Debug.Log($"<color=green>[Interaction]</color> Agent {agentController.name} used {objectName}. Remaining uses: {remainingUses}.");

            // Start the cooldown to become available again
            if (remainingUses > 0)
            {
                Invoke(nameof(ResetCooldown), cooldownDuration);
            }
            else
            {
                Debug.Log($"<color=orange>[Interaction]</color> {objectName} is depleted.");
            }
        }
    }

    private void ResetCooldown()
    {
        isAvailable = true;
        Debug.Log($"<color=cyan>[Interaction]</color> {objectName} is available again.");
    }
}
