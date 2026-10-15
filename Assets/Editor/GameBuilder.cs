using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// One-shot builder: creates Assets/Scenes/Game.unity (player ship, asteroid field, follow camera, HUD, pause menu)
// and Assets/Prefabs/Bullet.prefab.
public static class GameBuilder
{
    [MenuItem("Tools/Build Game Scene")]
    public static void Build()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var mat = AssetDatabase.LoadAssetAtPath<Material>(
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");

        var cam = Camera.main;
        cam.orthographicSize = 7;
        cam.backgroundColor = new Color(0.02f, 0.03f, 0.09f);
        float h = cam.orthographicSize, w = h * 16f / 9f;

        MainMenuBuilder.InitResources();

        // Endless parallax stars: a tiled sprite (needs Full Rect mesh) repositioned by Starfield.
        var starSprite = MainMenuBuilder.LoadSprite("Assets/Sprites/StarsTile.png", 120);
        var ti = (TextureImporter)AssetImporter.GetAtPath("Assets/Sprites/StarsTile.png");
        var tis = new TextureImporterSettings();
        ti.ReadTextureSettings(tis);
        tis.spriteMeshType = SpriteMeshType.FullRect;
        ti.SetTextureSettings(tis);
        ti.SaveAndReimport();
        starSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/StarsTile.png");
        var stars = MainMenuBuilder.MakeSprite("Stars", null, starSprite, mat, -10);
        var starRenderer = stars.GetComponent<SpriteRenderer>();
        starRenderer.drawMode = SpriteDrawMode.Tiled;
        starRenderer.size = starSprite.bounds.size * 6;
        stars.AddComponent<Starfield>();

        // Player
        var ship = MainMenuBuilder.MakeSprite("PlayerShip", null,
            MainMenuBuilder.LoadSprite("Assets/Sprites/Ship5_Flanker.png", 128), mat, 10);
        var rb = ship.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearDamping = 0.8f;
        rb.angularDamping = 5f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // keeps OnTriggerStay2D firing while hovering over a rock
        ship.AddComponent<PolygonCollider2D>();
        ship.transform.rotation = Quaternion.Euler(0, 0, 90); // start nose-up, so W flies up the screen
        var player = ship.AddComponent<PlayerShip>();
        MainMenuBuilder.AddEngineTrail(ship, mat);

        cam.gameObject.AddComponent<CameraFollow>();
        Assign(cam.GetComponent<CameraFollow>(), "target", ship.transform);

        // Asteroids: Drift wraps them around the camera view, so wherever the ship flies there are always rocks ahead.
        var rockSprite = MainMenuBuilder.LoadSprite("Assets/Sprites/Asteroid1_Rock.png", 100);
        var field = new GameObject("Asteroids");
        for (int i = 0; i < 14; i++)
        {
            Vector2 pos;
            do pos = new Vector2(Random.Range(-w, w), Random.Range(-h, h));
            while (pos.magnitude < 4f); // don't start on top of the player

            float scale = Random.Range(0.6f, 1.6f);
            var rock = MainMenuBuilder.MakeSprite($"Asteroid{i + 1}", field.transform, rockSprite, mat, 0);
            rock.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
            rock.transform.localScale = Vector3.one * scale;
            rock.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            var col = rock.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
            rock.AddComponent<Drift>();
        }

        // HUD: 3 hearts (health), top-left
        var hud = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
        hud.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = hud.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var lives = new GameObject("Lives", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        lives.transform.SetParent(hud.transform, false);
        var lrt = (RectTransform)lives.transform;
        lrt.anchorMin = lrt.anchorMax = lrt.pivot = new Vector2(0, 1);
        lrt.anchoredPosition = new Vector2(40, -40);
        lrt.sizeDelta = new Vector2(240, 64);
        lives.GetComponent<HorizontalLayoutGroup>().spacing = 12;

        var heartSprite = MainMenuBuilder.LoadSprite("Assets/Sprites/Health/Heart_Full.png", 100);
        var hearts = new Image[3];
        for (int i = 0; i < hearts.Length; i++)
        {
            var heart = new GameObject($"Heart{i + 1}", typeof(RectTransform), typeof(Image));
            heart.transform.SetParent(lives.transform, false);
            hearts[i] = heart.GetComponent<Image>();
            hearts[i].sprite = heartSprite;
            hearts[i].preserveAspect = true;
            hearts[i].raycastTarget = false;
        }

        // Pause menu (ESC)
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        hud.AddComponent<GraphicRaycaster>();
        var pausePanel = MainMenuBuilder.Panel("PausePanel", hud.transform, MainMenuBuilder.OverlayColor);
        var card = MainMenuBuilder.Card(pausePanel, 700, 520);
        MainMenuBuilder.MakeText("Title", card, "PAUSED", 72, 110).color = MainMenuBuilder.TitleColor;
        var resume = MainMenuBuilder.MakeButton("ResumeButton", card, "Resume");
        var quit = MainMenuBuilder.MakeButton("MainMenuButton", card, "Main Menu");
        var pause = hud.AddComponent<PauseMenu>();
        Assign(pause, "pausePanel", pausePanel.gameObject);
        UnityEventTools.AddPersistentListener(resume.onClick, pause.Resume);
        UnityEventTools.AddPersistentListener(quit.onClick, pause.QuitToMenu);
        pausePanel.gameObject.SetActive(false);

        var so = new SerializedObject(player);
        so.FindProperty("bulletPrefab").objectReferenceValue = MakeBulletPrefab(mat);
        so.FindProperty("emptyHeart").objectReferenceValue =
            MainMenuBuilder.LoadSprite("Assets/Sprites/Health/Heart_Empty.png", 100);
        var arr = so.FindProperty("hearts");
        arr.arraySize = hearts.Length;
        for (int i = 0; i < hearts.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = hearts[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Game.unity");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true),
        };
    }

    static GameObject MakeBulletPrefab(Material mat)
    {
        var go = MainMenuBuilder.MakeSprite("Bullet", null,
            MainMenuBuilder.LoadSprite("Assets/Sprites/Bullet.png", 100), mat, 9);
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        go.AddComponent<BoxCollider2D>().isTrigger = true;
        go.AddComponent<Bullet>();
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Bullet.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static void Assign(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
