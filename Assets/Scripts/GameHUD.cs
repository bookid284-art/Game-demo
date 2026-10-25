using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 准星、目标、电量、体力、阅读与胜负界面。
/// </summary>
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    public MonsterWatcherAI monster;
    public BasicFirstPersonController player;
    public FlashlightController flashlight;
    public PlayerInventory inventory;
    public PlayerInteractor interactor;

    Font font;
    Text objectiveText;
    Text keyText;
    Text promptText;
    Text toastText;
    Text watchText;
    Text hintText;
    Text batteryLabel;
    Text staminaLabel;
    Image batteryFill;
    Image staminaFill;
    Image crosshair;
    Image vignette;
    GameObject noteRoot;
    Text noteTitle;
    Text noteBody;
    GameObject journalRoot;
    Text journalBody;
    GameObject codeRoot;
    Text codeTitle;
    Text codeDigits;
    GameObject pauseRoot;
    GameObject deathRoot;
    GameObject winRoot;
    Text winTitle;
    Text winBody;
    GameObject introRoot;
    float toastTimer;
    float hintTimer = 22f;

    public static Sprite WhiteSprite()
    {
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Color[] pixels = { Color.white, Color.white, Color.white, Color.white };
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 100f);
    }

    void Awake()
    {
        Instance = this;
        font = GameFonts.Get();
        Build();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (objectiveText != null && GameManager.Instance != null)
            objectiveText.text = "目标  " + GameManager.Instance.ObjectiveText();
        if (keyText != null && inventory != null)
            keyText.text = "钥匙  " + inventory.ListKeys();
        if (flashlight != null && batteryFill != null)
        {
            batteryFill.fillAmount = flashlight.Battery01;
            batteryFill.color = flashlight.Battery01 < 0.2f
                ? new Color(0.85f, 0.25f, 0.2f)
                : new Color(1f, 0.78f, 0.42f);
            batteryLabel.text = flashlight.isOn ? "手电筒" : "手电筒（关）";
        }
        if (player != null && staminaFill != null)
            staminaFill.fillAmount = player.Stamina01;

        string prompt = interactor != null ? interactor.CurrentPrompt : string.Empty;
        if (promptText != null)
            promptText.text = prompt ?? string.Empty;

        bool showWatch = monster != null && monster.IsFrozen && monster.DistanceToPlayer < 22f
            && GameManager.Instance != null && GameManager.Instance.State == GameManager.RunState.Playing;
        if (watchText != null)
            watchText.gameObject.SetActive(showWatch);

        if (crosshair != null)
            crosshair.color = string.IsNullOrEmpty(prompt)
                ? new Color(1f, 1f, 1f, 0.85f)
                : new Color(1f, 0.78f, 0.35f, 1f);

        if (vignette != null && monster != null)
        {
            float near = Mathf.Clamp01(1f - monster.DistanceToPlayer / 8f);
            if (monster.IsFrozen)
                near *= 0.35f;
            if (!monster.IsHunting)
                near *= 0.25f;
            vignette.color = new Color(0.55f, 0.05f, 0.04f, near * 0.75f);
        }

        if (toastTimer > 0f)
        {
            toastTimer -= Time.unscaledDeltaTime;
            if (toastText != null)
            {
                Color color = toastText.color;
                color.a = Mathf.Clamp01(toastTimer);
                toastText.color = color;
            }
        }

        if (hintText != null)
        {
            hintTimer -= Time.deltaTime;
            Color color = hintText.color;
            color.a = Mathf.Clamp01(hintTimer / 4f) * 0.75f;
            hintText.color = color;
        }

        if (introRoot != null && GameManager.Instance != null)
            introRoot.SetActive(GameManager.Instance.State == GameManager.RunState.Intro);
    }

    public void Toast(string message)
    {
        if (toastText == null)
            return;
        toastText.text = message;
        toastText.color = new Color(0.95f, 0.93f, 0.88f, 1f);
        toastTimer = 3.4f;
    }

    public void ShowNote(string title, string body)
    {
        if (noteRoot == null)
            return;
        noteTitle.text = title;
        noteBody.text = body + "\n\n按 E 或 Esc 放下";
        noteRoot.SetActive(true);
    }

    public void HideNote()
    {
        if (noteRoot != null)
            noteRoot.SetActive(false);
    }

    public void ShowPause(bool visible)
    {
        if (pauseRoot != null)
            pauseRoot.SetActive(visible);
    }

    public void ShowDeath()
    {
        HideOverlays();
        ShowPause(false);
        if (deathRoot != null)
            deathRoot.SetActive(true);
    }

    public void ShowWin(string title, string body)
    {
        HideOverlays();
        ShowPause(false);
        if (winTitle != null)
            winTitle.text = title;
        if (winBody != null)
            winBody.text = body + "\n\n按 R 再值一次班";
        if (winRoot != null)
            winRoot.SetActive(true);
    }

    public void HideOverlays()
    {
        HideNote();
        HideJournal();
        HideCode();
    }

    public void ShowJournal(string body)
    {
        if (journalRoot == null)
            return;
        journalBody.text = body;
        journalRoot.SetActive(true);
    }

    public void HideJournal()
    {
        if (journalRoot != null)
            journalRoot.SetActive(false);
    }

    public void ShowCode(string title, string digits)
    {
        if (codeRoot == null)
            return;
        codeTitle.text = title;
        codeDigits.text = digits.Length == 0 ? "____" : digits;
        codeRoot.SetActive(true);
    }

    public void HideCode()
    {
        if (codeRoot != null)
            codeRoot.SetActive(false);
    }

    void Build()
    {
        Sprite sprite = WhiteSprite();
        GameObject canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(transform, false);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        vignette = FullImage(canvasGo.transform, "Vignette", sprite, new Color(0.4f, 0f, 0f, 0f));
        vignette.raycastTarget = false;

        objectiveText = Label(canvasGo.transform, "Objective", "目标", 24, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -28f), new Vector2(980f, 72f),
            new Color(0.93f, 0.9f, 0.82f));
        objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
        objectiveText.verticalOverflow = VerticalWrapMode.Truncate;
        keyText = Label(canvasGo.transform, "Keys", "钥匙  无", 22, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -104f), new Vector2(980f, 56f),
            new Color(0.95f, 0.78f, 0.4f));
        keyText.horizontalOverflow = HorizontalWrapMode.Wrap;

        batteryLabel = Label(canvasGo.transform, "BatteryLabel", "手电筒", 22, TextAnchor.UpperRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48f, -36f), new Vector2(280f, 32f),
            new Color(0.9f, 0.88f, 0.82f));
        batteryFill = Bar(canvasGo.transform, "Battery", sprite, new Vector2(-48f, -72f), new Color(1f, 0.78f, 0.42f));
        staminaLabel = Label(canvasGo.transform, "StaminaLabel", "体力", 22, TextAnchor.UpperRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48f, -104f), new Vector2(280f, 28f),
            new Color(0.85f, 0.88f, 0.9f));
        staminaFill = Bar(canvasGo.transform, "Stamina", sprite, new Vector2(-48f, -136f), new Color(0.65f, 0.82f, 0.95f));

        crosshair = Image(canvasGo.transform, "Crosshair", sprite, new Color(1f, 1f, 1f, 0.85f));
        RectTransform crossRect = crosshair.rectTransform;
        crossRect.anchorMin = crossRect.anchorMax = new Vector2(0.5f, 0.5f);
        crossRect.sizeDelta = new Vector2(6f, 6f);
        crossRect.anchoredPosition = Vector2.zero;
        crosshair.raycastTarget = false;

        watchText = Label(canvasGo.transform, "Watch", "不要移开视线", 30, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(600f, 48f),
            new Color(0.9f, 0.25f, 0.2f));
        watchText.gameObject.SetActive(false);

        promptText = Label(canvasGo.transform, "Prompt", "", 28, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(900f, 48f),
            new Color(0.95f, 0.93f, 0.86f));
        toastText = Label(canvasGo.transform, "Toast", "", 26, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 210f), new Vector2(1100f, 44f),
            new Color(0.95f, 0.93f, 0.88f, 0f));
        hintText = Label(canvasGo.transform, "Hint", "WASD 移动    Shift 冲刺    E 交互    F 手电筒    J 夜班记录    Esc 暂停",
            20, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f),
            new Vector2(1400f, 32f), new Color(0.8f, 0.8f, 0.8f, 0.75f));

        noteRoot = Panel(canvasGo.transform, "Note", sprite, new Color(0.08f, 0.07f, 0.06f, 0.92f),
            new Vector2(820f, 460f));
        noteTitle = Label(noteRoot.transform, "Title", "", 36, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(740f, 56f),
            new Color(0.95f, 0.86f, 0.62f));
        noteBody = Label(noteRoot.transform, "Body", "", 26, TextAnchor.UpperLeft,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(720f, 300f),
            new Color(0.92f, 0.9f, 0.84f));
        noteBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        noteBody.verticalOverflow = VerticalWrapMode.Overflow;
        noteRoot.SetActive(false);

        journalRoot = Panel(canvasGo.transform, "Journal", sprite, new Color(0.05f, 0.06f, 0.07f, 0.94f),
            new Vector2(760f, 760f));
        Label(journalRoot.transform, "Title", "夜班记录", 36, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(680f, 48f),
            new Color(0.95f, 0.86f, 0.62f));
        journalBody = Label(journalRoot.transform, "Body", "", 24, TextAnchor.UpperLeft,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(680f, 640f),
            new Color(0.9f, 0.9f, 0.86f));
        journalBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        journalBody.verticalOverflow = VerticalWrapMode.Overflow;
        journalRoot.SetActive(false);

        codeRoot = Panel(canvasGo.transform, "Code", sprite, new Color(0.04f, 0.05f, 0.06f, 0.94f),
            new Vector2(640f, 280f));
        codeTitle = Label(codeRoot.transform, "Title", "门禁", 34, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(560f, 48f),
            new Color(0.8f, 0.95f, 0.85f));
        codeDigits = Label(codeRoot.transform, "Digits", "____", 54, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(520f, 80f),
            new Color(0.95f, 0.92f, 0.8f));
        Label(codeRoot.transform, "Hint", "数字键输入    回车确认    Esc 取消", 22, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(560f, 36f),
            new Color(0.75f, 0.78f, 0.8f));
        codeRoot.SetActive(false);

        introRoot = new GameObject("Intro");
        introRoot.transform.SetParent(canvasGo.transform, false);
        Stretch(introRoot.AddComponent<RectTransform>());
        Label(introRoot.transform, "Title", "夜班医院", 72, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(800f, 100f),
            new Color(0.93f, 0.9f, 0.82f));
        Label(introRoot.transform, "Sub", "四个章，然后跑", 32, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(600f, 48f),
            new Color(0.75f, 0.28f, 0.22f));

        pauseRoot = Panel(canvasGo.transform, "Pause", sprite, new Color(0f, 0f, 0f, 0.72f), new Vector2(520f, 280f));
        Label(pauseRoot.transform, "Title", "暂停", 42, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(400f, 60f),
            Color.white);
        MakeButton(pauseRoot.transform, "继续", new Vector2(0f, 10f), delegate { GameManager.Instance.Resume(); });
        MakeButton(pauseRoot.transform, "重新开始", new Vector2(0f, -70f), delegate { GameManager.Instance.Restart(); });
        pauseRoot.SetActive(false);

        deathRoot = Panel(canvasGo.transform, "Death", sprite, new Color(0.08f, 0f, 0f, 0.82f), new Vector2(640f, 300f));
        Label(deathRoot.transform, "Title", "它抓住了你", 46, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(560f, 70f),
            new Color(0.9f, 0.3f, 0.25f));
        Label(deathRoot.transform, "Sub", "视线离开的时候，它在动。按 R 再试一次。", 24, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(560f, 60f),
            new Color(0.92f, 0.88f, 0.84f));
        MakeButton(deathRoot.transform, "再试一次", new Vector2(0f, -80f), delegate { GameManager.Instance.Restart(); });
        deathRoot.SetActive(false);

        winRoot = Panel(canvasGo.transform, "Win", sprite, new Color(0.02f, 0.05f, 0.04f, 0.88f), new Vector2(820f, 420f));
        winTitle = Label(winRoot.transform, "Title", "你逃出了医院", 42, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(740f, 64f),
            new Color(0.75f, 0.9f, 0.8f));
        winBody = Label(winRoot.transform, "Sub", "", 24, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(720f, 180f),
            new Color(0.9f, 0.92f, 0.9f));
        winBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        MakeButton(winRoot.transform, "再玩一次", new Vector2(0f, -150f), delegate { GameManager.Instance.Restart(); });
        winRoot.SetActive(false);

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject events = new GameObject("EventSystem");
            events.transform.SetParent(transform, false);
            events.AddComponent<UnityEngine.EventSystems.EventSystem>();
            events.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    Image Bar(Transform parent, string name, Sprite sprite, Vector2 anchored, Color color)
    {
        Image background = Image(parent, name + "Back", sprite, new Color(0f, 0f, 0f, 0.45f));
        RectTransform backRect = background.rectTransform;
        backRect.anchorMin = backRect.anchorMax = new Vector2(1f, 1f);
        backRect.pivot = new Vector2(1f, 1f);
        backRect.sizeDelta = new Vector2(220f, 14f);
        backRect.anchoredPosition = anchored;
        background.raycastTarget = false;

        Image fill = Image(background.transform, name + "Fill", sprite, color);
        RectTransform fillRect = fill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 1f;
        fill.raycastTarget = false;
        return fill;
    }

    GameObject Panel(Transform parent, string name, Sprite sprite, Color color, Vector2 size)
    {
        Image image = Image(parent, name, sprite, color);
        RectTransform rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        return image.gameObject;
    }

    void MakeButton(Transform parent, string caption, Vector2 anchored, UnityEngine.Events.UnityAction action)
    {
        Image image = Image(parent, caption, WhiteSprite(), new Color(0.18f, 0.16f, 0.15f, 1f));
        RectTransform rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(240f, 48f);
        rect.anchoredPosition = anchored;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        Text text = Label(image.transform, "Text", caption, 24, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        RectTransform textRect = text.rectTransform;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    Image FullImage(Transform parent, string name, Sprite sprite, Color color)
    {
        Image image = Image(parent, name, sprite, color);
        Stretch(image.rectTransform);
        return image;
    }

    Image Image(Transform parent, string name, Sprite sprite, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    Text Label(Transform parent, string name, string content, int size, TextAnchor anchor,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchored, Vector2 delta, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text text = go.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchored;
        rect.sizeDelta = delta;
        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
        return text;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
