using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MichaelHealthHUD : MonoBehaviour
{
    private static MichaelHealthHUD instance;
    private Canvas canvas;
    private TextMeshProUGUI healthLabel;
    private Michael targetMichael;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        GameObject hudObject = new GameObject("MichaelHealthHUD");
        DontDestroyOnLoad(hudObject);
        hudObject.AddComponent<MichaelHealthHUD>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildHud();
        targetMichael = FindFirstObjectByType<Michael>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (targetMichael == null)
        {
            targetMichael = FindFirstObjectByType<Michael>();
        }

        bool hasMichael = targetMichael != null && targetMichael.gameObject.activeInHierarchy;
        canvas.enabled = hasMichael;

        if (!hasMichael)
        {
            return;
        }

        healthLabel.text = $"Health: {Mathf.Max(0, targetMichael.health)}";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        targetMichael = FindFirstObjectByType<Michael>();
    }

    private void BuildHud()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        Transform existingPanel = transform.Find("HealthPanel");
        GameObject panelObject = existingPanel != null ? existingPanel.gameObject : new GameObject("HealthPanel");
        panelObject.transform.SetParent(transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            panelRect = panelObject.AddComponent<RectTransform>();
        }

        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-24f, -24f);
        panelRect.sizeDelta = new Vector2(340f, 70f);

        Image background = panelObject.GetComponent<Image>();
        if (background == null)
        {
            background = panelObject.AddComponent<Image>();
        }

        background.color = new Color(0f, 0f, 0f, 0.55f);

        Transform existingLabel = panelObject.transform.Find("HealthLabel");
        GameObject labelObject = existingLabel != null ? existingLabel.gameObject : new GameObject("HealthLabel");
        labelObject.transform.SetParent(panelObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        if (labelRect == null)
        {
            labelRect = labelObject.AddComponent<RectTransform>();
        }

        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        healthLabel = labelObject.GetComponent<TextMeshProUGUI>();
        if (healthLabel == null)
        {
            healthLabel = labelObject.AddComponent<TextMeshProUGUI>();
        }

        healthLabel.fontSize = 32f;
        healthLabel.alignment = TextAlignmentOptions.TopRight;
        healthLabel.color = Color.white;
        healthLabel.text = string.Empty;
        healthLabel.margin = new Vector4(16f, 10f, 16f, 10f);

        if (TMP_Settings.defaultFontAsset != null)
        {
            healthLabel.font = TMP_Settings.defaultFontAsset;
        }
        else
        {
            TMP_FontAsset fallbackFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (fallbackFont != null)
            {
                healthLabel.font = fallbackFont;
            }
        }
    }
}
