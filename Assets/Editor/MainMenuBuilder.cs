using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// One-shot builder: creates Assets/Scenes/MainMenu.unity. Run via batchmode or the menu item.
public static class MainMenuBuilder
{
    public static void ImportTmp() =>
        AssetDatabase.ImportPackage("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage", false);

    static TMP_DefaultControls.Resources res;
    static DefaultControls.Resources uiRes;

    // Space theme
    internal static readonly Color TitleColor = new(0.55f, 0.85f, 1f);
    internal static readonly Color OverlayColor = new(0.01f, 0.02f, 0.06f, 0.6f);
    static readonly Color ButtonColor = new(0.1f, 0.16f, 0.32f, 0.85f);
    static readonly Color CardColor = new(0.06f, 0.09f, 0.2f, 1f);
    static readonly Color KeyColor = new(0.85f, 0.9f, 1f);

    // Default UI sprites used by the UI helpers below; call before using them.
    internal static void InitResources()
    {
        uiRes = new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
        };
        res = new TMP_DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
        };
    }

    [MenuItem("Tools/Build Main Menu")]
    public static void Build()
    {
        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
        {
            ImportTmp();
            Debug.LogWarning("Imported TMP Essential Resources. Run Tools > Build Main Menu again.");
            return;
        }

        InitResources();

        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var menu = canvasGo.AddComponent<MainMenu>();

        // Main menu
        var mainPanel = Panel("MainMenuPanel", canvasGo.transform, new Color(0, 0, 0, 0));
        var title = MakeText("Title", mainPanel, "MY GAME", 120, 200);
        title.color = TitleColor;
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 12;
        var start = MakeButton("StartButton", mainPanel, "Start Game");
        var instr = MakeButton("InstructionButton", mainPanel, "Instruction");
        var sett = MakeButton("SettingsButton", mainPanel, "Settings");

        // Instruction popup: dim overlay + window with a scrollable guide (image left, text right)
        var instrPanel = Panel("InstructionPanel", canvasGo.transform, OverlayColor);
        var instrCard = Card(instrPanel, 1300, 950);
        MakeText("Title", instrCard, "HOW TO PLAY", 72, 100).color = TitleColor;
        var guide = ScrollView(instrCard, 1180, 620);

        var shipSprite = LoadSprite("Assets/Sprites/Ship5_Flanker.png", 128);
        var rockSprite = LoadSprite("Assets/Sprites/Asteroid1_Rock.png", 100);
        var heart = LoadSprite("Assets/Sprites/Health/Heart_Full.png", 100);

        Heading(guide, "GOAL");
        Icon(Entry(guide, "Pilot your fighter through the asteroid field. Shoot asteroids to score points and survive as long as you can.", 130), shipSprite, 150, 1f);

        Heading(guide, "RULES");
        var lives = CenterRow(Entry(guide, "You have 3 lives per game.", 90));
        for (int i = 0; i < 3; i++) Icon(lives, heart, 56, 1f);
        Icon(Entry(guide, "Hitting an asteroid costs 1 life.", 100), rockSprite, 90, 1f);
        Icon(Entry(guide, "After a hit, the ship blinks and can pass through objects for a moment.", 110), shipSprite, 130, 0.4f);

        Heading(guide, "CONTROLS");
        var wasd = Entry(guide, "W / S: fly forward / backward.\nA / D: turn left / right.", 160);
        KeyCap(CenterRow(wasd), "W", 70);
        var asd = CenterRow(wasd);
        KeyCap(asd, "A", 70); KeyCap(asd, "S", 70); KeyCap(asd, "D", 70);
        KeyCap(CenterRow(Entry(guide, "Shoot.", 90)), "SPACE", 220);
        KeyCap(CenterRow(Entry(guide, "Dash toward the direction you hold, e.g. Shift + A dodges left. Shift alone dashes forward.", 90)), "SHIFT", 180);
        KeyCap(CenterRow(Entry(guide, "Pause the game.", 90)), "ESC", 120);

        var instrBack = MakeButton("BackButton", instrCard, "Back");

        // Settings panel
        var settPanel = Panel("SettingsPanel", canvasGo.transform, OverlayColor);
        var settCard = Card(settPanel, 1100, 600);
        MakeText("Title", settCard, "SETTINGS", 72, 100).color = TitleColor;
        var settings = canvasGo.AddComponent<SettingsMenu>();
        var musicSource = new GameObject("Music").AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = true;

        var musicRow = Row(settCard, 40);
        Label(musicRow, "Music", 36, 320, 70);
        var musicToggle = MakeToggle(musicRow);
        var musicSlider = MakeSlider(musicRow, 420);
        musicSlider.value = 0.7f;
        var sfxRow = Row(settCard, 40);
        Label(sfxRow, "Sound Effects", 36, 320, 70);
        var sfxToggle = MakeToggle(sfxRow);
        var sfxSlider = MakeSlider(sfxRow, 420);
        sfxSlider.value = 1f;
        var fsRow = Row(settCard, 40);
        Label(fsRow, "Fullscreen", 36, 320, 70);
        var fsToggle = MakeToggle(fsRow);
        Layout(new GameObject("Spacer", typeof(RectTransform)), 420, 10).transform.SetParent(fsRow, false); // keeps rows aligned

        var settBack = MakeButton("BackButton", settCard, "Back");

        var sso = new SerializedObject(settings);
        sso.FindProperty("music").objectReferenceValue = musicSource;
        sso.FindProperty("musicToggle").objectReferenceValue = musicToggle;
        sso.FindProperty("musicSlider").objectReferenceValue = musicSlider;
        sso.FindProperty("sfxToggle").objectReferenceValue = sfxToggle;
        sso.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
        sso.FindProperty("fullscreenToggle").objectReferenceValue = fsToggle;
        sso.ApplyModifiedPropertiesWithoutUndo();

        instrPanel.gameObject.SetActive(false);
        settPanel.gameObject.SetActive(false);

        var so = new SerializedObject(menu);
        so.FindProperty("instructionPanel").objectReferenceValue = instrPanel.gameObject;
        so.FindProperty("settingsPanel").objectReferenceValue = settPanel.gameObject;
        so.FindProperty("gameScene").stringValue = "Game";
        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(start.onClick, menu.StartGame);
        UnityEventTools.AddPersistentListener(instr.onClick, menu.OpenInstruction);
        UnityEventTools.AddPersistentListener(sett.onClick, menu.OpenSettings);
        UnityEventTools.AddPersistentListener(instrBack.onClick, menu.Back);
        UnityEventTools.AddPersistentListener(settBack.onClick, menu.Back);

        AddBackground();

        const string path = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(path, true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true),
        };
    }

    // Adds the animated space background to MainMenu.unity without touching the UI.
    // Safe to re-run: replaces only the "SpaceBackground" object.
    [MenuItem("Tools/Add Menu Background")]
    public static void BuildBackground()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        AddBackground();
        EditorSceneManager.SaveScene(scene);
    }

    static void AddBackground()
    {
        var old = GameObject.Find("SpaceBackground");
        if (old) Object.DestroyImmediate(old);

        var mat = AssetDatabase.LoadAssetAtPath<Material>(
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
        var cam = Camera.main;
        cam.backgroundColor = new Color(0.02f, 0.03f, 0.09f);
        float h = cam.orthographicSize, w = h * cam.aspect;

        var root = new GameObject("SpaceBackground");

        MakeSprite("Stars", root.transform, LoadSprite("Assets/Sprites/Stars.png", 120), mat, -10);

        var rockSprite = LoadSprite("Assets/Sprites/Asteroid1_Rock.png", 100);
        for (int i = 0; i < 7; i++)
        {
            float scale = Random.Range(0.5f, 1.3f);
            var rock = MakeSprite($"Asteroid{i + 1}", root.transform, rockSprite, mat, 0);
            rock.transform.position = new Vector3(Random.Range(-w, w), Random.Range(-h, h), 0);
            rock.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            rock.transform.localScale = Vector3.one * scale;
            float tint = Mathf.Lerp(0.45f, 1f, Mathf.InverseLerp(0.5f, 1.3f, scale)); // small = farther = dimmer
            rock.GetComponent<SpriteRenderer>().color = new Color(tint, tint, tint);
            rock.AddComponent<Drift>();
        }

        var ship = MakeSprite("Ship", root.transform, LoadSprite("Assets/Sprites/Ship5_Flanker.png", 128), mat, 10);
        ship.transform.position = new Vector3(-100, 0, 0); // off screen until the first fly-by
        ship.AddComponent<ShipFlyBy>();

        AddEngineTrail(ship, mat);
    }

    internal static void AddEngineTrail(GameObject ship, Material mat)
    {
        var engine = new GameObject("EngineTrail");
        engine.transform.SetParent(ship.transform, false);
        engine.transform.localPosition = new Vector3(-0.95f, 0, 0);
        var trail = engine.AddComponent<TrailRenderer>();
        trail.sharedMaterial = mat;
        trail.time = 0.35f;
        trail.startWidth = 0.35f;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.05f;
        trail.sortingOrder = 9;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.75f, 0.3f), 0), new GradientColorKey(new Color(1f, 0.25f, 0.05f), 1) },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        trail.colorGradient = g;
    }

    internal static Sprite LoadSprite(string path, float ppu)
    {
        AssetDatabase.ImportAsset(path);
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = ppu;
        ti.mipmapEnabled = false;
        ti.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    internal static GameObject MakeSprite(string name, Transform parent, Sprite sprite, Material mat, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = mat;
        sr.sortingOrder = order;
        return go;
    }

    internal static Transform Panel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        go.GetComponent<Image>().color = color;
        var lg = go.GetComponent<VerticalLayoutGroup>();
        lg.childAlignment = TextAnchor.MiddleCenter;
        lg.spacing = 30;
        lg.childControlWidth = lg.childControlHeight = true;
        lg.childForceExpandWidth = lg.childForceExpandHeight = false;
        return go.transform;
    }

    internal static Button MakeButton(string name, Transform parent, string label)
    {
        var go = TMP_DefaultControls.CreateButton(res);
        go.name = name;
        go.transform.SetParent(parent, false);
        var text = go.GetComponentInChildren<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 40;
        text.color = Color.white;
        go.GetComponent<Image>().color = ButtonColor;
        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(0.85f, 0.85f, 0.85f);
        colors.highlightedColor = colors.selectedColor = Color.white;
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f);
        button.colors = colors;
        Layout(go, 400, 90);
        return button;
    }

    internal static TextMeshProUGUI MakeText(string name, Transform parent, string content, float size, float height)
    {
        var go = TMP_DefaultControls.CreateText(res);
        go.name = name;
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        Layout(go, 1200, height);
        return t;
    }

    static GameObject Layout(GameObject go, float w, float h)
    {
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = w;
        le.preferredHeight = h;
        return go;
    }

    internal static Transform Card(Transform parent, float w, float h)
    {
        var go = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = uiRes.standard;
        img.type = Image.Type.Sliced;
        img.color = CardColor;
        var lg = go.GetComponent<VerticalLayoutGroup>();
        lg.padding = new RectOffset(60, 60, 40, 40);
        lg.spacing = 30;
        lg.childAlignment = TextAnchor.UpperCenter;
        lg.childControlWidth = lg.childControlHeight = true;
        lg.childForceExpandWidth = lg.childForceExpandHeight = false;
        Layout(go, w, h);
        return go.transform;
    }

    static Transform Row(Transform parent, float spacing)
    {
        var go = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        go.transform.SetParent(parent, false);
        var lg = go.GetComponent<HorizontalLayoutGroup>();
        lg.spacing = spacing;
        lg.childAlignment = TextAnchor.MiddleLeft;
        lg.childControlWidth = lg.childControlHeight = true;
        lg.childForceExpandWidth = lg.childForceExpandHeight = false;
        return go.transform;
    }

    static void Heading(Transform parent, string text)
    {
        var t = Label(parent, text, 40, 600, 60);
        t.color = TitleColor;
        t.fontStyle = FontStyles.Bold;
    }

    static TextMeshProUGUI Label(Transform parent, string text, float size, float w, float h)
    {
        var t = MakeText("Label", parent, text, size, h);
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.GetComponent<LayoutElement>().preferredWidth = w;
        return t;
    }

    static void KeyCap(Transform parent, string key, float w)
    {
        var go = new GameObject($"Key_{key}", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = uiRes.standard;
        img.type = Image.Type.Sliced;
        img.color = KeyColor;
        Layout(go, w, 70);
        var t = MakeText("Text", go.transform, key, 30, 70);
        t.color = new Color(0.08f, 0.1f, 0.2f);
        t.fontStyle = FontStyles.Bold;
        var rt = t.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
    }

    // One guide line: fixed-width image box on the left, explanation on the right. Returns the image box.
    static Transform Entry(Transform parent, string text, float h)
    {
        var row = Row(parent, 40);
        var box = new GameObject("Image", typeof(RectTransform), typeof(VerticalLayoutGroup));
        box.transform.SetParent(row, false);
        var lg = box.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 8;
        lg.childAlignment = TextAnchor.MiddleCenter;
        lg.childControlWidth = lg.childControlHeight = true;
        lg.childForceExpandWidth = lg.childForceExpandHeight = false;
        Layout(box, 260, h);
        Label(row, text, 30, 820, h);
        return box.transform;
    }

    static Transform CenterRow(Transform parent)
    {
        var row = Row(parent, 8);
        row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        return row;
    }

    // Vertical-only ScrollRect with a scrollbar on the right. Returns the content transform.
    static Transform ScrollView(Transform parent, float w, float h)
    {
        var sv = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        sv.transform.SetParent(parent, false);
        Layout(sv, w, h);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        viewport.transform.SetParent(sv.transform, false);
        viewport.GetComponent<Image>().color = Color.clear; // catches drag/scroll input
        var vrt = (RectTransform)viewport.transform;
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = Vector2.zero;
        vrt.offsetMax = new Vector2(-36, 0);

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var crt = (RectTransform)content.transform;
        crt.anchorMin = new Vector2(0, 1);
        crt.anchorMax = Vector2.one;
        crt.pivot = new Vector2(0.5f, 1);
        crt.sizeDelta = Vector2.zero;
        var lg = content.GetComponent<VerticalLayoutGroup>();
        lg.padding = new RectOffset(10, 10, 10, 30);
        lg.spacing = 24;
        lg.childAlignment = TextAnchor.UpperLeft;
        lg.childControlWidth = lg.childControlHeight = true;
        lg.childForceExpandWidth = lg.childForceExpandHeight = false;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var bar = DefaultControls.CreateScrollbar(uiRes).GetComponent<Scrollbar>();
        bar.transform.SetParent(sv.transform, false);
        bar.direction = Scrollbar.Direction.BottomToTop;
        var brt = (RectTransform)bar.transform;
        brt.anchorMin = new Vector2(1, 0);
        brt.anchorMax = Vector2.one;
        brt.pivot = new Vector2(1, 0.5f);
        brt.sizeDelta = new Vector2(20, 0);
        brt.anchoredPosition = Vector2.zero;
        bar.GetComponent<Image>().color = new Color(1, 1, 1, 0.15f);
        bar.handleRect.GetComponent<Image>().color = TitleColor;

        var sr = sv.GetComponent<ScrollRect>();
        sr.content = crt;
        sr.viewport = vrt;
        sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 40;
        sr.verticalScrollbar = bar;
        return content.transform;
    }

    static void Icon(Transform parent, Sprite sprite, float size, float alpha)
    {
        var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.color = new Color(1, 1, 1, alpha);
        img.raycastTarget = false;
        Layout(go, size, size);
    }

    static Toggle MakeToggle(Transform parent)
    {
        var go = DefaultControls.CreateToggle(uiRes);
        go.transform.SetParent(parent, false);
        Object.DestroyImmediate(go.transform.Find("Label").gameObject);
        var bg = (RectTransform)go.transform.Find("Background");
        bg.anchorMin = Vector2.zero;
        bg.anchorMax = Vector2.one;
        bg.anchoredPosition = Vector2.zero;
        bg.sizeDelta = Vector2.zero;
        var check = (RectTransform)bg.Find("Checkmark");
        check.anchorMin = Vector2.zero;
        check.anchorMax = Vector2.one;
        check.anchoredPosition = Vector2.zero;
        check.sizeDelta = new Vector2(-10, -10);
        check.GetComponent<Image>().color = new Color(0.1f, 0.3f, 0.6f);
        Layout(go, 56, 56);
        return go.GetComponent<Toggle>();
    }

    static Slider MakeSlider(Transform parent, float w)
    {
        var slider = DefaultControls.CreateSlider(uiRes).GetComponent<Slider>();
        slider.transform.SetParent(parent, false);
        slider.fillRect.GetComponent<Image>().color = TitleColor;
        Layout(slider.gameObject, w, 40);
        return slider;
    }
}
