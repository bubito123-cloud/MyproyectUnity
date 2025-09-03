using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NarratorEngine : MonoBehaviour
{
    [Header("Narration Settings")]
    public bool enableNarration = true;
    public float narrationCooldown = 2f;

    [Header("Template Configuration")]
    public bool useDetailedNarration = true;
    public bool includeEmotionalContext = true;
    public bool includeUtilityInfo = false; // For debugging

    // Internal state
    private float lastNarrationTime = 0f;
    private Dictionary<string, object> templates;
    private Dictionary<string, string> lastNarrationByType;

    private void Awake()
    {
        // Inicializaciones obligatorias
        if (lastNarrationByType == null)
            lastNarrationByType = new Dictionary<string, string>();

        if (templates == null)
            InitializeTemplates();
    }

    private void InitializeTemplates()
    {
        templates = new Dictionary<string, object>();

        // Initialize templates (lista simple)
        templates["init"] = new List<string>
        {
            "I am awakening in this space. Time to understand my environment.",
            "Systems online. Beginning exploration and learning.",
            "New session started. I feel ready to explore and achieve goals."
        };

        templates["episode_start"] = new List<string>
        {
            "Starting fresh. Let me approach this differently.",
            "New episode begins. I'll apply what I've learned.",
            "Reset complete. Time for a new strategy."
        };

        // Templates con subcategorías (diccionario)
        templates["action"] = new Dictionary<string, List<string>>
        {
            ["moving_forward"] = new List<string>
            {
                "Moving forward with purpose.",
                "Advancing toward my objective.",
                "Progress feels good."
            },
            ["moving_backward"] = new List<string>
            {
                "Stepping back to reassess.",
                "Retreating to find a better angle.",
                "Sometimes backing up is the right move."
            },
            ["turning"] = new List<string>
            {
                "Adjusting my perspective.",
                "Looking for new opportunities.",
                "A different direction might be better."
            },
            ["waiting"] = new List<string>
            {
                "Taking a moment to think.",
                "Patience can be a strategy too.",
                "Observing before acting."
            },
            ["exploring"] = new List<string>
            {
                "This area seems interesting.",
                "Curiosity drives me forward.",
                "New territory to discover."
            }
        };

        templates["goal_reached"] = new List<string>
        {
            "Success! This feels rewarding.",
            "Achievement unlocked. I'm getting better.",
            "Goal accomplished. Satisfaction increases.",
            "That worked well. I'll remember this approach."
        };

        templates["collision"] = new Dictionary<string, List<string>>
        {
            ["wall"] = new List<string>
            {
                "Ouch. That wall came out of nowhere.",
                "Note to self: walls are solid.",
                "Frustrating obstacle. I need to be more careful.",
                "This barrier is teaching me patience."
            },
            ["obstacle"] = new List<string>
            {
                "Another obstacle in my path.",
                "These barriers test my persistence.",
                "I need to find a way around this.",
                "Obstacles make victory sweeter."
            }
        };

        templates["stuck"] = new List<string>
        {
            "I seem to be repeating myself. Time to try something new.",
            "This pattern isn't working. Let me change approach.",
            "Stuck in a loop. Breaking free with a random move.",
            "Sometimes the best action is to do something unexpected."
        };

        templates["emotional"] = new Dictionary<string, List<string>>
        {
            ["high_motivation"] = new List<string>
            {
                "I feel energized and ready for action!",
                "My motivation is peaked. Let's achieve something great.",
                "Energy flowing through me. Time to make progress."
            },
            ["high_frustration"] = new List<string>
            {
                "This is getting frustrating. I need to stay calm.",
                "Obstacles everywhere. But I won't give up.",
                "Frustration builds, but so does my determination."
            },
            ["high_curiosity"] = new List<string>
            {
                "So many unexplored possibilities here.",
                "My curiosity is leading me to new discoveries.",
                "What's around the next corner? I must find out."
            },
            ["high_satisfaction"] = new List<string>
            {
                "Things are going well. I'm pleased with my progress.",
                "A sense of accomplishment fills me.",
                "Satisfaction makes every step lighter."
            }
        };

        templates["timeout"] = new List<string>
        {
            "Time's up for this attempt. Let me start over.",
            "Episode limit reached. Time to reset and try again.",
            "This round ends. Next one will be better."
        };
    }

    public string GenerateNarration(string eventType, string context, EmotionalState emotions, float utility = 0f)
    {
        // Protecciones y normalizaciones
        if (!enableNarration) return "";

        if (templates == null)
        {
            Debug.LogWarning("NarratorEngine: templates es null, llamando a InitializeTemplates()");
            InitializeTemplates();
        }

        if (lastNarrationByType == null)
            lastNarrationByType = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(eventType))
        {
            Debug.LogWarning("NarratorEngine.GenerateNarration recibió eventType null/empty. Normalizando a 'unknown'.");
            eventType = "unknown";
        }

        if (emotions == null)
        {
            // Fallback seguro
            emotions = new EmotionalState();
        }

        // Cooldown check
        if (Time.time - lastNarrationTime < narrationCooldown)
        {
            if (lastNarrationByType.ContainsKey(eventType))
                return lastNarrationByType[eventType];
            else
                return "";
        }

        string narration = "";

        try
        {
            switch (eventType)
            {
                case "action":
                    narration = GenerateActionNarration(context, emotions);
                    break;
                case "collision":
                    narration = GenerateCollisionNarration(context, emotions);
                    break;
                case "emotional":
                    narration = GenerateEmotionalNarration(emotions);
                    break;
                default:
                    narration = GenerateGenericNarration(eventType, context, emotions);
                    break;
            }

            // Add emotional context if enabled
            if (includeEmotionalContext && emotions != null)
            {
                narration = AddEmotionalContext(narration, emotions);
            }

            // Add utility info for debugging
            if (includeUtilityInfo)
            {
                narration += $" [Utility: {utility:F2}]";
            }

            lastNarrationTime = Time.time;
            // Guardamos con la key ya normalizada (no nula)
            lastNarrationByType[eventType] = narration;

            return narration;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Narration generation failed: {e.Message}\n{e.StackTrace}");
            return "I'm thinking...";
        }
    }

    private string GenerateActionNarration(string actionType, EmotionalState emotions)
    {
        if (templates != null &&
            templates.ContainsKey("action") &&
            templates["action"] is Dictionary<string, List<string>> actionTemplates &&
            actionTemplates.ContainsKey(actionType))
        {
            var templateList = actionTemplates[actionType];

            // Select template based on emotional state
            int index = SelectTemplateByEmotion(templateList.Count, emotions);
            index = Mathf.Clamp(index, 0, Mathf.Max(0, templateList.Count - 1));
            return templateList[index];
        }

        return $"I am {actionType}.";
    }

    private string GenerateCollisionNarration(string obstacleType, EmotionalState emotions)
    {
        if (templates != null &&
            templates.ContainsKey("collision") &&
            templates["collision"] is Dictionary<string, List<string>> collisionTemplates &&
            collisionTemplates.ContainsKey(obstacleType))
        {
            var templateList = collisionTemplates[obstacleType];

            float frustration = (emotions != null) ? emotions.frustration : 0f;
            int index = Mathf.RoundToInt((frustration / 100f) * (templateList.Count - 1));
            index = Mathf.Clamp(index, 0, Mathf.Max(0, templateList.Count - 1));
            return templateList[index];
        }

        return $"I hit a {obstacleType}.";
    }

    private string GenerateEmotionalNarration(EmotionalState emotions)
    {
        string dominantEmotion = GetDominantEmotionName(emotions);
        string templateKey = $"high_{dominantEmotion}";

        if (templates != null &&
            templates.ContainsKey("emotional") &&
            templates["emotional"] is Dictionary<string, List<string>> emotionalTemplates &&
            emotionalTemplates.ContainsKey(templateKey))
        {
            var templateList = emotionalTemplates[templateKey];
            int index = UnityEngine.Random.Range(0, templateList.Count);
            return templateList[index];
        }

        return $"I'm feeling particularly {dominantEmotion} right now.";
    }

    private string GenerateGenericNarration(string eventType, string context, EmotionalState emotions)
    {
        if (templates != null && templates.ContainsKey(eventType) && templates[eventType] is List<string> templateList && templateList.Count > 0)
        {
            int index = SelectTemplateByEmotion(templateList.Count, emotions);
            index = Mathf.Clamp(index, 0, templateList.Count - 1);

            string template = templateList[index];

            // Simple template variable replacement
            if (!string.IsNullOrEmpty(context))
                template = template.Replace("[CONTEXT]", context);
            template = template.Replace("[EMOTION]", GetDominantEmotionName(emotions));

            return template;
        }

        return $"Something happened: {eventType}";
    }

    private string AddEmotionalContext(string baseNarration, EmotionalState emotions)
    {
        if (!includeEmotionalContext) return baseNarration;

        string dominantEmotion = GetDominantEmotionName(emotions);
        float dominantValue = GetDominantEmotionValue(emotions);

        if (dominantValue > 75f)
        {
            return $"{baseNarration} I'm feeling very {dominantEmotion}.";
        }
        else if (dominantValue > 50f)
        {
            return $"{baseNarration} There's some {dominantEmotion} in me.";
        }

        return baseNarration;
    }

    private int SelectTemplateByEmotion(int templateCount, EmotionalState emotions)
    {
        if (emotions == null || templateCount <= 1) return 0;

        float emotionalBias = (emotions.curiosity - emotions.frustration) / 100f;
        float randomFactor = UnityEngine.Random.Range(0f, 1f);
        float selection = Mathf.Clamp01((emotionalBias + randomFactor) / 2f);

        return Mathf.RoundToInt(selection * (templateCount - 1));
    }

    private string GetDominantEmotionName(EmotionalState emotions)
    {
        if (emotions == null) return "neutral";

        var emotionValues = new Dictionary<string, float>
        {
            { "motivated", emotions.motivation },
            { "satisfied", emotions.satisfaction },
            { "frustrated", emotions.frustration },
            { "curious", emotions.curiosity }
        };

        float maxValue = emotionValues.Values.Max();
        var dominantEmotions = emotionValues.Where(kvp => kvp.Value == maxValue).ToList();

        if (dominantEmotions.Count == 0) return "balanced";
        if (dominantEmotions.Count == 1) return dominantEmotions[0].Key;

        // Tie-breaker: randomly select one of the dominant emotions
        int randomIndex = UnityEngine.Random.Range(0, dominantEmotions.Count);
        return dominantEmotions[randomIndex].Key;
    }

    private float GetDominantEmotionValue(EmotionalState emotions)
    {
        if (emotions == null) return 50f;

        return Mathf.Max(emotions.motivation, emotions.satisfaction,
                        emotions.frustration, emotions.curiosity);
    }

    // Public methods for customization
    public void AddCustomTemplate(string eventType, string template)
    {
        if (templates == null) InitializeTemplates();

        if (!templates.ContainsKey(eventType))
        {
            templates[eventType] = new List<string>();
        }

        if (templates[eventType] is List<string> list)
        {
            list.Add(template);
        }
        else
        {
            Debug.LogWarning($"Cannot add custom template to '{eventType}': not a List<string>.");
        }
    }

    public void SetNarrationCooldown(float cooldown)
    {
        narrationCooldown = cooldown;
    }

    // Context menu debug methods
    [ContextMenu("Test Narration")]
    public void TestNarration()
    {
        var testEmotions = new EmotionalState();
        testEmotions.motivation = 80f;
        testEmotions.curiosity = 60f;

        string test = GenerateNarration("action", "moving_forward", testEmotions);
        Debug.Log($"Test narration: {test}");
    }

    [ContextMenu("Print All Templates")]
    public void PrintAllTemplates()
    {
        if (templates == null) InitializeTemplates();

        foreach (var category in templates.Keys)
        {
            Debug.Log($"Category: {category}");
            if (templates[category] is List<string> stringList)
            {
                foreach (string template in stringList)
                {
                    Debug.Log($"  - {template}");
                }
            }
            else if (templates[category] is Dictionary<string, List<string>> dictTemplates)
            {
                foreach (var subCategory in dictTemplates.Keys)
                {
                    Debug.Log($"  Subcategory: {subCategory}");
                    foreach (string template in dictTemplates[subCategory])
                    {
                        Debug.Log($"    - {template}");
                    }
                }
            }
            else
            {
                Debug.Log($"  (unrecognized template type for category '{category}')");
            }
        }
    }
}
