using UnityEngine;

/// <summary>
/// Base class for any object an agent can interact with.
/// FIX: Added a virtual OnInteract method so subclasses like FoodSource can override it.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    /// <summary>
    /// The core interaction method. This is meant to be overridden by child classes.
    /// It is declared as virtual so that other classes can provide their own implementation.
    /// </summary>
    /// <param name="controller">The cognitive controller of the agent that is interacting.</param>
    public virtual void OnInteract(CognitiveController controller)
    {
        // Default behavior: do nothing.
        // Child classes like FoodSource or Key will have specific logic here.
        Debug.Log($"<color=yellow>[InteractableObject]</color> {gameObject.name} was interacted with, but has no specific OnInteract behavior.");
    }
}
