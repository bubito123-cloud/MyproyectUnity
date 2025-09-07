using UnityEngine;

/// <summary>
/// A simple door that can be opened.
/// </summary>
public class Door : MonoBehaviour
{
    /// <summary>
    /// Opens the door, making it passable.
    /// For simplicity, this just deactivates the door object.
    /// </summary>
    public void Open()
    {
        Debug.Log($"Door {name} has been opened!");
        gameObject.SetActive(false);
    }
}
