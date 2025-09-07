using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an object that has been detected by the agent's sensors.
/// </summary>
public class PerceivedObject
{
    public Transform transform;
    public string tag;

    public PerceivedObject(Transform t)
    {
        transform = t;
        tag = t.tag;
    }
}

/// <summary>
/// Acts as a registry for objects detected by the agent's trigger collider.
/// This system is now passive and relies on the agent's physics events.
/// </summary>
public class PerceptionSystem : MonoBehaviour
{
    private List<PerceivedObject> perceivedObjects = new List<PerceivedObject>();

    /// <summary>
    /// Registers an object that has entered the agent's perception range.
    /// </summary>
    public void RegisterObject(Transform objTransform)
    {
        if (objTransform != null && !perceivedObjects.Any(o => o.transform == objTransform))
        {
            perceivedObjects.Add(new PerceivedObject(objTransform));
        }
    }

    /// <summary>
    /// De-registers an object that has exited the agent's perception range.
    /// </summary>
    public void DeregisterObject(Transform objTransform)
    {
        if (objTransform != null)
        {
            perceivedObjects.RemoveAll(o => o.transform == objTransform);
        }
    }

    /// <summary>
    /// Returns the list of all objects currently perceived by the agent.
    /// </summary>
    public List<PerceivedObject> GetPerceivedObjects()
    {
        // We can add a check here to remove null objects if they get destroyed
        perceivedObjects.RemoveAll(o => o.transform == null);
        return perceivedObjects;
    }

    /// <summary>
    /// Checks if a specific GameObject is currently in the list of perceived objects.
    /// </summary>
    public bool IsVisible(GameObject obj)
    {
        if (obj == null) return false;
        return perceivedObjects.Any(o => o.transform == obj.transform);
    }
}
