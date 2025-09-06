using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class NarrationEvent : UnityEvent<string> { }

public class NarratorEngine : MonoBehaviour
{
    public NarrationEvent OnNarrate;

    private string lastNarratedText = "";

    public void Narrate(string message)
    {
        // --- FIX: Prevent repeating the same message over and over ---
        if (message == lastNarratedText)
        {
            return; // Do nothing if the thought is the same as the last one.
        }

        lastNarratedText = message;
        if(OnNarrate != null)
        {
            OnNarrate.Invoke(message);
        }
    }

    // Call this to allow a message to be repeated after a reset.
    public void ClearHistory()
    {
        lastNarratedText = "";
    }
}
