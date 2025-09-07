using UnityEngine;

/// <summary>
/// A switch that can be activated by an agent to trigger an effect, like opening a door.
/// </summary>
public class Switch : MonoBehaviour
{
    [Header("Switch Configuration")]
    [Tooltip("The door that this switch operates.")]
    public Door targetDoor;

    /// <summary>
    /// Activates the switch, which in turn tells its target door to open.
    /// </summary>
    public void Activate()
    {
        if (targetDoor != null)
        {
            Debug.Log($"Switch {name} activated, opening door {targetDoor.name}.");
            targetDoor.Open();
        }
        else
        {
            Debug.LogWarning($"Switch {name} was activated, but has no target door assigned.");
        }
    }
}
