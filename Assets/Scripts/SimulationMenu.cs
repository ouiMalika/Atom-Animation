using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Creates a World Space UI panel with Anneal / Tensile buttons.
/// Attach to any GameObject. Assign moleculeLoader reference.
/// Buttons have Box Colliders so QuestPointer can raycast them.
/// </summary>
public class SimulationMenu : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public MoleculeLoader moleculeLoader;

    // Exposed so QuestPointer can find and highlight them
    [HideInInspector] public Button annealButton;
    [HideInInspector] public Button tensileButton;
    [HideInInspector] public Button activeHover;

    // Canvas world position: 1.8 m high, 1.2 m in front of user, slight tilt down
    private readonly Vector3 menuPosition = new Vector3(0f, 1.8f, -1.2f);
    private readonly Vector3 menuEuler = new Vector3(10f, 0f, 0f); // tilt toward user

    // Visual state
    private Color normalColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
    private Color hoverColor  = new Color(0.30f, 0.55f, 0.90f, 0.95f);
    private Color pressColor  = new Color(0.10f, 0.38f, 0.70f, 1.00f);
    private Color activeColor = new Color(0.10f, 0.60f, 0.30f, 0.95f); // loaded

    private Image annealImg;
    private Image tensileImg;
    private string currentSim = "";

    void Awake()
    {
        EnsureEventSystem();
        BuildCanvas();
    }

    void Start()
    {
        // Auto-load first simulation on start
        OnAnneal();
    }

    // ── Called by QuestPointer ────────────────────────────────────────────────

    public void SetHover(Button btn)
    {
        if (activeHover == btn) return;
        ClearHover();
        activeHover = btn;
        if (btn != null)
            GetButtonImage(btn).color = hoverColor;
    }

    public void ClearHover()
    {
        if (annealImg != null) annealImg.color = (currentSim == "anneal") ? activeColor : normalColor;
        if (tensileImg != null) tensileImg.color = (currentSim == "tensile") ? activeColor : normalColor;
        activeHover = null;
    }

    public void PressHovered()
    {
        if (activeHover == null) return;
        GetButtonImage(activeHover).color = pressColor;
        activeHover.onClick.Invoke();
    }

    // ── Button callbacks ──────────────────────────────────────────────────────

    void OnAnneal()
    {
        currentSim = "anneal";
        ClearHover();
        if (moleculeLoader != null) moleculeLoader.LoadSimulation("anneal");
    }

    void OnTensile()
    {
        currentSim = "tensile";
        ClearHover();
        if (moleculeLoader != null) moleculeLoader.LoadSimulation("tensile");
    }

    // ── Canvas builder ────────────────────────────────────────────────────────

    void BuildCanvas()
    {
        // Root canvas object
        GameObject canvasGO = new GameObject("SimulationMenuCanvas");
        canvasGO.transform.SetParent(transform, false);
        canvasGO.transform.position = menuPosition;
        canvasGO.transform.rotation = Quaternion.Euler(menuEuler);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;

        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400f, 220f);
        canvasRect.localScale = Vector3.one * 0.002f; // 0.002 → 0.8 m wide

        // Background panel
        GameObject panelGO = MakeImage(canvasGO, "Panel", new Rect(0, 0, 400, 220),
            new Color(0.05f, 0.05f, 0.05f, 0.80f));
        AddCollider(panelGO, new Vector2(400, 220));

        // Title
        MakeText(panelGO, "Title", "SIMULATION SELECT",
            new Rect(0, 60, 380, 50), 28, Color.white, TextAnchor.MiddleCenter);

        // Anneal button
        GameObject annealGO = MakeImage(panelGO, "AnnealBtn", new Rect(-100, -30, 160, 60), normalColor);
        MakeText(annealGO, "Label", "ANNEAL", new Rect(0, 0, 160, 60), 22, Color.white, TextAnchor.MiddleCenter);
        annealButton = annealGO.AddComponent<Button>();
        annealButton.onClick.AddListener(OnAnneal);
        annealImg = annealGO.GetComponent<Image>();
        AddCollider(annealGO, new Vector2(160, 60));

        // Tensile button
        GameObject tensileGO = MakeImage(panelGO, "TensileBtn", new Rect(100, -30, 160, 60), normalColor);
        MakeText(tensileGO, "Label", "TENSILE", new Rect(0, 0, 160, 60), 22, Color.white, TextAnchor.MiddleCenter);
        tensileButton = tensileGO.AddComponent<Button>();
        tensileButton.onClick.AddListener(OnTensile);
        tensileImg = tensileGO.GetComponent<Image>();
        AddCollider(tensileGO, new Vector2(160, 60));

        // Status text (updated by ClearHover via currentSim)
        MakeText(panelGO, "Status", "Point controller at a button and pull trigger",
            new Rect(0, -90, 380, 30), 14, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleCenter);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    Image GetButtonImage(Button btn)
    {
        if (btn == annealButton) return annealImg;
        if (btn == tensileButton) return tensileImg;
        return btn.GetComponent<Image>();
    }

    static GameObject MakeImage(GameObject parent, string name, Rect rect, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(rect.x, rect.y);
        rt.sizeDelta = new Vector2(rect.width, rect.height);
        return go;
    }

    static void MakeText(GameObject parent, string name, string content,
        Rect rect, int size, Color color, TextAnchor align)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Text txt = go.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = size;
        txt.color = color;
        txt.alignment = align;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(rect.x, rect.y);
        rt.sizeDelta = new Vector2(rect.width, rect.height);
    }

    static void AddCollider(GameObject go, Vector2 size)
    {
        BoxCollider col = go.AddComponent<BoxCollider>();
        // Canvas is in UI units scaled by 0.002; collider must match rect size
        col.size = new Vector3(size.x, size.y, 2f);
        col.center = Vector3.zero;
    }

    static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}
