using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmotionalDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Canvas emotionalCanvas;
    public TextMeshProUGUI narrationText;
    public Slider motivationSlider;
    public Slider satisfactionSlider;
    public Slider frustrationSlider;
    public Slider curiositySlider;

    [Header("Visual Feedback")]
    public Image emotionalColorIndicator;
    public ParticleSystem emotionalParticles;
    public Light emotionalLight;

    [Header("Settings")]
    public bool followAgent = true;
    public Vector3 canvasOffset = new Vector3(0, 3, 0);
    public float updateRate = 0.1f;

    // Internal state
    private Transform agentTransform;
    private float lastUpdateTime = 0f;
    private EmotionalState lastEmotions;
    private string lastNarration = "";

    // Color schemes for emotions
    private readonly Color motivationColor = new Color(0.2f, 0.8f, 0.3f); // Green
    private readonly Color satisfactionColor = new Color(0.3f, 0.6f, 1f); // Blue
    private readonly Color frustrationColor = new Color(1f, 0.3f, 0.2f); // Red
    private readonly Color curiosityColor = new Color(1f, 0.8f, 0.2f); // Yellow

    private void Start()
    {
        agentTransform = GetComponent<Transform>();

        // Create UI if not assigned
        if (emotionalCanvas == null)
        {
            CreateEmotionalUI();
        }

        InitializeSliders();

        lastEmotions = new EmotionalState();
    }

    private void CreateEmotionalUI()
    {
        // Create canvas
        GameObject canvasGO = new GameObject("EmotionalCanvas");
        canvasGO.transform.SetParent(transform);
        emotionalCanvas = canvasGO.AddComponent<Canvas>();
        emotionalCanvas.renderMode = RenderMode.WorldSpace;
        emotionalCanvas.worldCamera = Camera.main;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 0.01f;

        // Create main panel
        GameObject panelGO = new GameObject("EmotionalPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 300);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);

        // Create narration text
        GameObject narrationGO = new GameObject("NarrationText");
        narrationGO.transform.SetParent(panelGO.transform, false);

        RectTransform narrationRect = narrationGO.AddComponent<RectTransform>();
        narrationRect.sizeDelta = new Vector2(380, 80);
        narrationRect.anchoredPosition = new Vector2(0, 100);

        narrationText = narrationGO.AddComponent<TextMeshProUGUI>();
        narrationText.text = "Initializing...";
        narrationText.fontSize = 14;
        narrationText.color = Color.white;
        narrationText.alignment = TextAlignmentOptions.Center;
        narrationText.textWrappingMode = TextWrappingModes.Normal;

        // Create sliders
        CreateEmotionSlider("Motivation", motivationColor, new Vector2(0, 40), ref motivationSlider, panelGO);
        CreateEmotionSlider("Satisfaction", satisfactionColor, new Vector2(0, 10), ref satisfactionSlider, panelGO);
        CreateEmotionSlider("Frustration", frustrationColor, new Vector2(0, -20), ref frustrationSlider, panelGO);
        CreateEmotionSlider("Curiosity", curiosityColor, new Vector2(0, -50), ref curiositySlider, panelGO);

        // Create color indicator
        GameObject colorGO = new GameObject("ColorIndicator");
        colorGO.transform.SetParent(panelGO.transform, false);

        RectTransform colorRect = colorGO.AddComponent<RectTransform>();
        colorRect.sizeDelta = new Vector2(50, 50);
        colorRect.anchoredPosition = new Vector2(0, -100);

        emotionalColorIndicator = colorGO.AddComponent<Image>();
        emotionalColorIndicator.color = Color.white;
    }

    private void CreateEmotionSlider(string emotionName, Color color, Vector2 position, ref Slider slider, GameObject parent)
    {
        GameObject sliderGO = new GameObject($"{emotionName}Slider");
        sliderGO.transform.SetParent(parent.transform, false);

        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(300, 20);
        sliderRect.anchoredPosition = position;

        slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 50f;

        // Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(300, 20);
        bgRect.anchoredPosition = Vector2.zero;
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        slider.targetGraphic = bgImage;

        // Fill area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.sizeDelta = new Vector2(-20, 0);
        fillAreaRect.anchoredPosition = Vector2.zero;

        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = color;

        slider.fillRect = fillRect;

        // Handle
        GameObject handleAreaGO = new GameObject("Handle Slide Area");
        handleAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform handleAreaRect = handleAreaGO.AddComponent<RectTransform>();
        handleAreaRect.sizeDelta = new Vector2(-20, 0);
        handleAreaRect.anchoredPosition = Vector2.zero;

        GameObject handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(handleAreaGO.transform, false);
        RectTransform handleRect = handleGO.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);
        Image handleImage = handleGO.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.handleRect = handleRect;

        // Label
        GameObject labelGO = new GameObject($"{emotionName}Label");
        labelGO.transform.SetParent(sliderGO.transform, false);
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(80, 20);
        labelRect.anchoredPosition = new Vector2(-160, 0);

        TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = emotionName;
        labelText.fontSize = 10;
        labelText.color = color;
        labelText.alignment = TextAlignmentOptions.Right;
    }

    private void InitializeSliders()
    {
        if (motivationSlider != null) motivationSlider.value = 50f;
        if (satisfactionSlider != null) satisfactionSlider.value = 50f;
        if (frustrationSlider != null) frustrationSlider.value = 0f;
        if (curiositySlider != null) curiositySlider.value = 70f;
    }

    public void UpdateDisplay(EmotionalState emotions, string narration)
    {
        if (Time.time - lastUpdateTime < updateRate) return;

        lastEmotions = emotions;
        lastNarration = narration;

        // Update sliders
        if (motivationSlider != null)
            motivationSlider.value = Mathf.Lerp(motivationSlider.value, emotions.motivation, Time.deltaTime * 2f);
        if (satisfactionSlider != null)
            satisfactionSlider.value = Mathf.Lerp(satisfactionSlider.value, emotions.satisfaction, Time.deltaTime * 2f);
        if (frustrationSlider != null)
            frustrationSlider.value = Mathf.Lerp(frustrationSlider.value, emotions.frustration, Time.deltaTime * 2f);
        if (curiositySlider != null)
            curiositySlider.value = Mathf.Lerp(curiositySlider.value, emotions.curiosity, Time.deltaTime * 2f);

        // Update narration text
        if (narrationText != null && !string.IsNullOrEmpty(narration))
        {
            narrationText.text = narration;
        }

        // Update color indicator
        UpdateColorIndicator(emotions);

        // Update particles
        UpdateParticleEffects(emotions);

        // Update light
        UpdateEmotionalLight(emotions);

        // Update canvas position
        if (followAgent && emotionalCanvas != null)
        {
            emotionalCanvas.transform.position = agentTransform.position + canvasOffset;
            emotionalCanvas.transform.LookAt(Camera.main.transform);
        }

        lastUpdateTime = Time.time;
    }

    private void UpdateColorIndicator(EmotionalState emotions)
    {
        if (emotionalColorIndicator == null) return;

        // Blend colors based on emotional intensities
        Color blendedColor =
            motivationColor * (emotions.motivation / 100f) +
            satisfactionColor * (emotions.satisfaction / 100f) +
            frustrationColor * (emotions.frustration / 100f) +
            curiosityColor * (emotions.curiosity / 100f);

        blendedColor /= 4f; // Normalize
        blendedColor.a = 1f;

        emotionalColorIndicator.color = blendedColor;
    }

    private void UpdateParticleEffects(EmotionalState emotions)
    {
        if (emotionalParticles == null) return;

        var main = emotionalParticles.main;
        var emission = emotionalParticles.emission;

        // Particle count based on emotional intensity
        float totalEmotionalIntensity = (emotions.motivation + emotions.satisfaction +
                                       emotions.frustration + emotions.curiosity) / 4f;

        emission.rateOverTime = totalEmotionalIntensity / 10f;

        // Color based on dominant emotion
        string dominantEmotion = GetDominantEmotion(emotions);
        switch (dominantEmotion)
        {
            case "motivation":
                main.startColor = new ParticleSystem.MinMaxGradient(motivationColor);
                break;
            case "satisfaction":
                main.startColor = new ParticleSystem.MinMaxGradient(satisfactionColor);
                break;
            case "frustration":
                main.startColor = new ParticleSystem.MinMaxGradient(frustrationColor);
                break;
            case "curiosity":
                main.startColor = new ParticleSystem.MinMaxGradient(curiosityColor);
                break;
        }
    }

    private void UpdateEmotionalLight(EmotionalState emotions)
    {
        if (emotionalLight == null) return;

        // Light intensity based on motivation
        emotionalLight.intensity = emotions.motivation / 100f * 2f;

        // Light color based on dominant emotion
        string dominantEmotion = GetDominantEmotion(emotions);
        switch (dominantEmotion)
        {
            case "motivation":
                emotionalLight.color = motivationColor;
                break;
            case "satisfaction":
                emotionalLight.color = satisfactionColor;
                break;
            case "frustration":
                emotionalLight.color = frustrationColor;
                break;
            case "curiosity":
                emotionalLight.color = curiosityColor;
                break;
        }
    }

    private string GetDominantEmotion(EmotionalState emotions)
    {
        float maxValue = Mathf.Max(emotions.motivation, emotions.satisfaction,
                                   emotions.frustration, emotions.curiosity);

        if (maxValue == emotions.motivation) return "motivation";
        if (maxValue == emotions.satisfaction) return "satisfaction";
        if (maxValue == emotions.frustration) return "frustration";
        if (maxValue == emotions.curiosity) return "curiosity";

        return "balanced";
    }

    // Public methods for external control
    public void SetFollowAgent(bool follow)
    {
        followAgent = follow;
    }

    public void SetCanvasOffset(Vector3 offset)
    {
        canvasOffset = offset;
    }

    public void SetUpdateRate(float rate)
    {
        updateRate = Mathf.Max(0.01f, rate);
    }

    public void ShowDisplay(bool show)
    {
        if (emotionalCanvas != null)
        {
            emotionalCanvas.gameObject.SetActive(show);
        }
    }

    // Manual update method
    public void ForceUpdate(EmotionalState emotions, string narration)
    {
        lastUpdateTime = 0f; // Force update on next call
        UpdateDisplay(emotions, narration);
    }

    // Debug methods
    [ContextMenu("Test Display")]
    public void TestDisplay()
    {
        var testEmotions = new EmotionalState();
        testEmotions.motivation = UnityEngine.Random.Range(0f, 100f);
        testEmotions.satisfaction = UnityEngine.Random.Range(0f, 100f);
        testEmotions.frustration = UnityEngine.Random.Range(0f, 100f);
        testEmotions.curiosity = UnityEngine.Random.Range(0f, 100f);

        string testNarration = "This is a test narration for the emotional display system.";

        UpdateDisplay(testEmotions, testNarration);
    }

    [ContextMenu("Reset Display")]
    public void ResetDisplay()
    {
        var neutralEmotions = new EmotionalState();
        UpdateDisplay(neutralEmotions, "Display reset to neutral state.");
    }
}
