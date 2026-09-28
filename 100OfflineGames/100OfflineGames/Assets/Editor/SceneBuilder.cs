// SceneBuilder.cs
// Place this in Assets/Editor/ folder.
// In Unity: top menu -> Tools -> Build All Scenes
// This creates all 7 scenes automatically.

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class SceneBuilder : EditorWindow
{
    [MenuItem("Tools/Build All 100OfflineGames Scenes")]
    public static void BuildAllScenes()
    {
        BuildMainMenu();
        BuildCookieClicker();
        Build2040();
        BuildPaperSnake();
        BuildBobluxob();
        BuildMineBloxx();
        BuildVehicleSimulator();
        AddScenesToBuildSettings();
        Debug.Log("=== All scenes built! Check File > Build Settings. ===");
    }

    // ---------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------
    static string ScenesPath = "Assets/Scenes/";

    static UnityEngine.SceneManagement.Scene NewScene(string name)
    {
        Directory.CreateDirectory(ScenesPath);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        return scene;
    }

    static void SaveScene(UnityEngine.SceneManagement.Scene scene, string name)
    {
        EditorSceneManager.SaveScene(scene, ScenesPath + name + ".unity");
    }

    static Camera AddCamera(string camName = "Main Camera")
    {
        GameObject camGO = new GameObject(camName);
        Camera cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.backgroundColor = new Color(0.13f, 0.13f, 0.2f);
        camGO.AddComponent<AudioListener>();
        return cam;
    }

    static Canvas AddCanvas(string canvasName = "Canvas")
    {
        GameObject canvasGO = new GameObject(canvasName);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        // EventSystem
        GameObject esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        return canvas;
    }

    static GameObject AddPanel(Transform parent, string name, Color col,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        Image img = go.AddComponent<Image>();
        img.color = col;
        return go;
    }

    static Text AddText(Transform parent, string name, string content,
        int fontSize, Color col, TextAnchor anchor,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        Text t = go.AddComponent<Text>();
        t.text = content;
        t.fontSize = fontSize;
        t.color = col;
        t.alignment = anchor;
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return t;
    }

    static Button AddButton(Transform parent, string name, string label,
        Color bgCol, Color textCol, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        Image img = go.AddComponent<Image>();
        img.color = bgCol;
        Button btn = go.AddComponent<Button>();

        AddText(go.transform, "Label", label, fontSize, textCol,
            TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        return btn;
    }

    // ---------------------------------------------------------------
    // MAIN MENU
    // ---------------------------------------------------------------
    static void BuildMainMenu()
    {
        var scene = NewScene("MainMenu");
        AddCamera();

        // Background
        Canvas canvas = AddCanvas();
        Transform canvasT = canvas.transform;

        // Dark background
        AddPanel(canvasT, "Background", new Color(0.1f, 0.1f, 0.15f),
            Vector2.zero, Vector2.one, new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);

        // Title
        AddText(canvasT, "Title", "100 Offline Games", 48, Color.white,
            TextAnchor.MiddleCenter,
            new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);

        // Settings button (gear ⚙) top-right
        AddButton(canvasT, "SettingsButton", "⚙", 
            new Color(0.3f,0.3f,0.4f), Color.white, 36,
            new Vector2(0.88f, 0.89f), new Vector2(0.88f, 0.89f), new Vector2(80,80), Vector2.zero);

        // Settings Panel (hidden by default)
        GameObject settingsPanel = AddPanel(canvasT, "SettingsPanel", new Color(0.15f,0.15f,0.25f,0.97f),
            new Vector2(0.2f,0.2f), new Vector2(0.8f,0.8f), new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);
        settingsPanel.SetActive(false);

        AddText(settingsPanel.transform, "Title", "Settings", 36, Color.white,
            TextAnchor.UpperCenter,
            new Vector2(0,0.8f), new Vector2(1,0.95f), Vector2.zero, Vector2.zero);

        AddText(settingsPanel.transform, "MusicLabel", "Music Volume", 22, Color.white,
            TextAnchor.MiddleLeft,
            new Vector2(0.05f,0.65f), new Vector2(0.4f,0.72f), Vector2.zero, Vector2.zero);

        AddText(settingsPanel.transform, "SFXLabel", "SFX Volume", 22, Color.white,
            TextAnchor.MiddleLeft,
            new Vector2(0.05f,0.52f), new Vector2(0.4f,0.59f), Vector2.zero, Vector2.zero);

        AddText(settingsPanel.transform, "VibLabel", "Vibration", 22, Color.white,
            TextAnchor.MiddleLeft,
            new Vector2(0.05f,0.40f), new Vector2(0.4f,0.47f), Vector2.zero, Vector2.zero);

        AddButton(settingsPanel.transform, "CloseBtn", "Close",
            new Color(0.8f,0.2f,0.2f), Color.white, 26,
            new Vector2(0.3f,0.05f), new Vector2(0.7f,0.17f), Vector2.zero, Vector2.zero);

        // Games list (vertical, scrollable area)
        // Each game gets a button below the settings gear
        string[] gameNames = {
            "🍪  Cookie Clicker",
            "🔢  2040",
            "👾  Bobluxob",
            "⛏  MineBloxx",
            "🚗  Vehicle Simulator",
            "🐍  PaperSnake.io"
        };
        Color[] btnColors = {
            new Color(0.8f,0.5f,0.1f),
            new Color(0.2f,0.5f,0.8f),
            new Color(0.7f,0.1f,0.1f),
            new Color(0.1f,0.6f,0.2f),
            new Color(0.4f,0.2f,0.8f),
            new Color(0.1f,0.6f,0.7f),
        };

        float btnH = 0.10f;
        float gap  = 0.01f;
        float startY = 0.78f;

        for (int i = 0; i < gameNames.Length; i++)
        {
            float top = startY - i * (btnH + gap);
            float bot = top - btnH;
            AddButton(canvasT, "GameBtn_" + i, gameNames[i],
                btnColors[i], Color.white, 26,
                new Vector2(0.05f, bot), new Vector2(0.85f, top), Vector2.zero, Vector2.zero);
        }

        // Attach MainMenuUI component to a manager GO
        GameObject mgr = new GameObject("MainMenuManager");
        mgr.AddComponent<MainMenuUI>();
        // Wire up in-editor references would need SerializedObject manipulation;
        // user wires them in Inspector after scene is built.

        // GameManager
        GameObject gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();

        SaveScene(scene, "MainMenu");
        Debug.Log("MainMenu scene built.");
    }

    // ---------------------------------------------------------------
    // COOKIE CLICKER
    // ---------------------------------------------------------------
    static void BuildCookieClicker()
    {
        var scene = NewScene("CookieClicker");
        Camera cam = AddCamera();
        cam.backgroundColor = new Color(0.12f, 0.06f, 0.02f);
        Canvas canvas = AddCanvas();
        Transform ct = canvas.transform;

        // Brown background
        AddPanel(ct, "BG", new Color(0.2f, 0.1f, 0.05f),
            Vector2.zero, Vector2.one, new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);

        // Cookie count text at top
        Text cookieText = AddText(ct, "CookieCountText", "Cookies: 0", 36, Color.white,
            TextAnchor.MiddleCenter,
            new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.97f), Vector2.zero, Vector2.zero);

        // Cookie button in the middle (big circle-ish)
        Button cookieBtn = AddButton(ct, "CookieButton", "🍪",
            new Color(0.7f, 0.4f, 0.1f), Color.white, 80,
            new Vector2(0.3f, 0.35f), new Vector2(0.7f, 0.70f), Vector2.zero, Vector2.zero);

        // Upgrades toggle button top-right
        Button upgradesToggle = AddButton(ct, "UpgradesToggleButton", "🛒\nUpgrades",
            new Color(0.3f,0.5f,0.1f), Color.white, 20,
            new Vector2(0.78f, 0.88f), new Vector2(0.78f, 0.88f), new Vector2(100,80), Vector2.zero);

        // Upgrades panel
        GameObject upgradesPanel = AddPanel(ct, "UpgradesPanel", new Color(0.1f,0.08f,0.04f,0.97f),
            new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.85f), new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);
        upgradesPanel.SetActive(false);

        AddText(upgradesPanel.transform, "UpgradesTitle", "Upgrades", 30, Color.yellow,
            TextAnchor.UpperCenter,
            new Vector2(0,0.93f), new Vector2(1,1), Vector2.zero, Vector2.zero);

        // 7 upgrade buttons
        string[] upNames = {
            "+1 Click/Click  — 10 🍪",
            "+20 Click/Click — 30 🍪",
            "+100 Click/Click — 90 🍪",
            "+1 Auto/sec — 300 🍪",
            "+80 Auto/sec — 800 🍪",
            "Ultra Fun Mode — 10K 🍪",
            "Finish Game — 1M 🍪"
        };
        Button[] upBtns   = new Button[7];
        

        float slotH = 0.117f;
        for (int i = 0; i < 7; i++)
        {
            float top = 0.90f - i * slotH;
            float bot = top - slotH + 0.01f;
            Button b = AddButton(upgradesPanel.transform, "UpgradeBtn_" + i, upNames[i],
                new Color(0.25f,0.18f,0.08f), Color.white, 18,
                new Vector2(0.02f, bot), new Vector2(0.98f, top), Vector2.zero, Vector2.zero);
            upBtns[i] = b;
        }

        // HUD bar
        BuildInGameHUD(ct);

        // Attach CookieClicker script
        GameObject ccGO = new GameObject("CookieClickerController");
        CookieClicker cc = ccGO.AddComponent<CookieClicker>();
        cc.cookieCountText        = cookieText;
        cc.cookieButton           = cookieBtn;
        cc.upgradesToggleButton   = upgradesToggle;
        cc.upgradesPanel          = upgradesPanel;
        cc.upgradeButtons         = upBtns;

        SaveScene(scene, "CookieClicker");
        Debug.Log("CookieClicker scene built.");
    }

    // ---------------------------------------------------------------
    // 2040
    // ---------------------------------------------------------------
    static void Build2040()
    {
        var scene = NewScene("Game2040");
        Camera cam = AddCamera();
        cam.backgroundColor = new Color(0.47f, 0.43f, 0.40f);
        Canvas canvas = AddCanvas();
        Transform ct = canvas.transform;

        AddPanel(ct, "BG", new Color(0.47f,0.43f,0.40f),
            Vector2.zero, Vector2.one, new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);

        AddText(ct, "Title", "2040", 52, Color.white,
            TextAnchor.MiddleCenter,
            new Vector2(0.1f,0.88f), new Vector2(0.55f,0.97f), Vector2.zero, Vector2.zero);

        Text scoreText = AddText(ct, "ScoreText", "Score: 0", 26, Color.white,
            TextAnchor.MiddleLeft,
            new Vector2(0.05f,0.80f), new Vector2(0.55f,0.88f), Vector2.zero, Vector2.zero);

        Text bestText = AddText(ct, "BestText", "Best: 0", 26, Color.white,
            TextAnchor.MiddleLeft,
            new Vector2(0.05f,0.73f), new Vector2(0.55f,0.80f), Vector2.zero, Vector2.zero);

        Button newGameBtn = AddButton(ct, "NewGameButton", "New Game",
            new Color(0.6f,0.5f,0.4f), Color.white, 24,
            new Vector2(0.6f,0.80f), new Vector2(0.95f,0.92f), Vector2.zero, Vector2.zero);

        // 4x4 grid
        float gridLeft   = 0.05f;
        float gridBottom = 0.12f;
        float gridSize   = 0.65f;
        float cellSize   = gridSize / 4f;
        float gap        = 0.005f;

        Text[] tileTexts = new Text[16];
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                float left   = gridLeft + c * (cellSize + gap);
                float bottom = gridBottom + (3 - r) * (cellSize + gap);
                float right  = left + cellSize - gap;
                float top    = bottom + cellSize - gap;

                GameObject cell = AddPanel(ct, "Cell_" + r + "_" + c,
                    new Color(0.72f, 0.67f, 0.62f),
                    new Vector2(left, bottom), new Vector2(right, top),
                    new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

                Text t = AddText(cell.transform, "TileText", "", 42, new Color(0.47f,0.43f,0.40f),
                    TextAnchor.MiddleCenter,
                    Vector2.zero, Vector2.one, new Vector2(-4,-4), Vector2.zero);
                tileTexts[r * 4 + c] = t;
            }
        }

        // Swipe instruction
        AddText(ct, "Hint", "Swipe or WASD to move tiles", 18, new Color(0.7f,0.7f,0.7f),
            TextAnchor.MiddleCenter,
            new Vector2(0.05f,0.03f), new Vector2(0.95f,0.10f), Vector2.zero, Vector2.zero);

        BuildInGameHUD(ct);

        GameObject g = new GameObject("Game2040Controller");
        Game2040 g2 = g.AddComponent<Game2040>();
        g2.tileTexts    = tileTexts;
        g2.scoreText    = scoreText;
        g2.bestText     = bestText;
        g2.newGameButton = newGameBtn;

        SaveScene(scene, "Game2040");
        Debug.Log("Game2040 scene built.");
    }

    // ---------------------------------------------------------------
    // PAPERSNAKE
    // ---------------------------------------------------------------
    static void BuildPaperSnake()
    {
        var scene = NewScene("PaperSnake");
        Camera cam = AddCamera();
        cam.backgroundColor = new Color(0.9f,0.9f,0.9f);
        Canvas canvas = AddCanvas();
        Transform ct = canvas.transform;

        AddPanel(ct, "BG", new Color(0.92f,0.92f,0.92f),
            Vector2.zero, Vector2.one, new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);

        AddText(ct, "Title", "PaperSnake.io", 40, new Color(0.2f,0.2f,0.2f),
            TextAnchor.MiddleCenter,
            new Vector2(0.1f,0.90f), new Vector2(0.7f,0.98f), Vector2.zero, Vector2.zero);

        Text scoreText = AddText(ct, "ScoreText", "Territory: 0", 26, new Color(0.2f,0.2f,0.2f),
            TextAnchor.MiddleLeft,
            new Vector2(0.05f,0.83f), new Vector2(0.65f,0.90f), Vector2.zero, Vector2.zero);

        // Grid container
        GameObject gridGO = new GameObject("GridContainer");
        gridGO.transform.SetParent(ct, false);
        RectTransform gridRT = gridGO.AddComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0.05f, 0.10f);
        gridRT.anchorMax = new Vector2(0.95f, 0.82f);
        gridRT.sizeDelta = Vector2.zero;
        gridRT.anchoredPosition = Vector2.zero;

        Text gameOverText = AddText(ct, "GameOverText", "Game Over!", 48, Color.red,
            TextAnchor.MiddleCenter,
            new Vector2(0.1f,0.4f), new Vector2(0.9f,0.6f), Vector2.zero, Vector2.zero);
        gameOverText.gameObject.SetActive(false);

        Button restartBtn = AddButton(ct, "RestartButton", "Restart",
            new Color(0.2f,0.6f,1f), Color.white, 28,
            new Vector2(0.3f,0.3f), new Vector2(0.7f,0.42f), Vector2.zero, Vector2.zero);
        restartBtn.gameObject.SetActive(false);

        AddText(ct, "Hint", "Swipe or WASD to move", 18, new Color(0.5f,0.5f,0.5f),
            TextAnchor.MiddleCenter,
            new Vector2(0.1f,0.02f), new Vector2(0.9f,0.09f), Vector2.zero, Vector2.zero);

        BuildInGameHUD(ct);

        GameObject g = new GameObject("PaperSnakeController");
        PaperSnake ps = g.AddComponent<PaperSnake>();
        ps.gridContainer  = gridRT;
        ps.scoreText      = scoreText;
        ps.gameOverText   = gameOverText;
        ps.restartButton  = restartBtn;

        SaveScene(scene, "PaperSnake");
        Debug.Log("PaperSnake scene built.");
    }

    // ---------------------------------------------------------------
    // BOBLUXOB
    // ---------------------------------------------------------------
    static void BuildBobluxob()
    {
        var scene = NewScene("Bobluxob");

        // Camera
        GameObject camGO = new GameObject("Main Camera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f);
        cam.transform.position = new Vector3(0, 3, -8);
        cam.transform.eulerAngles = new Vector3(15, 0, 0);
        camGO.AddComponent<AudioListener>();

        // Directional light
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.eulerAngles = new Vector3(50, -30, 0);

        // Player
        GameObject playerGO = new GameObject("Player");
        playerGO.transform.position = new Vector3(0, 1f, 0);
        Rigidbody playerRb = playerGO.AddComponent<Rigidbody>();
        playerRb.freezeRotation = true;
        CapsuleCollider playerCol = playerGO.AddComponent<CapsuleCollider>();
        playerCol.height = 2.5f;
        playerCol.center = new Vector3(0, 1.25f, 0);

        // NPC parent
        GameObject npcParent = new GameObject("NPCs");
        // Item spawn parent
        GameObject itemParent = new GameObject("Items");

        // Canvas / HUD
        Canvas canvas = AddCanvas();
        Transform ct = canvas.transform;

        Text killFeed = AddText(ct, "KillFeedText", "", 24, Color.yellow,
            TextAnchor.MiddleCenter,
            new Vector2(0.1f,0.87f), new Vector2(0.9f,0.96f), Vector2.zero, Vector2.zero);

        // Hotbar (4 slots at bottom)
        Button[] hbBtns  = new Button[4];
        Text[]   hbNames = new Text[4];
        string[] items   = { "Sword", "Rocket", "Boombox", "Wall" };
        for (int i = 0; i < 4; i++)
        {
            float left = 0.04f + i * 0.24f;
            Button b = AddButton(ct, "HB_" + i, items[i],
                new Color(0.2f,0.2f,0.2f,0.85f), Color.white, 18,
                new Vector2(left, 0.02f), new Vector2(left + 0.22f, 0.13f), Vector2.zero, Vector2.zero);
            hbBtns[i] = b;
            hbNames[i] = b.GetComponentInChildren<Text>();
        }

        // Ragdoll button bottom-right
        Button ragdollBtn = AddButton(ct, "RagdollButton", "Ragdoll",
            new Color(0.8f,0.1f,0.1f), Color.white, 22,
            new Vector2(0.75f, 0.14f), new Vector2(0.98f, 0.24f), Vector2.zero, Vector2.zero);

        // Jump button
        Button jumpBtn = AddButton(ct, "JumpButton", "Jump",
            new Color(0.1f,0.5f,0.9f), Color.white, 22,
            new Vector2(0.75f, 0.25f), new Vector2(0.98f, 0.35f), Vector2.zero, Vector2.zero);

        BuildInGameHUD(ct);

        // Wire Bobluxob script
        Bobluxob bb = playerGO.AddComponent<Bobluxob>();
        bb.playerTransform  = playerGO.transform;
        bb.playerRb         = playerRb;
        bb.hotbarButtons    = hbBtns;
        bb.hotbarItemNames  = hbNames;
        bb.ragdollButton    = ragdollBtn;
        bb.jumpButton       = jumpBtn;
        bb.killFeedText     = killFeed;
        bb.npcParent        = npcParent.transform;
        bb.itemSpawnParent  = itemParent.transform;

        // Attach camera to player
        cam.transform.SetParent(playerGO.transform);

        SaveScene(scene, "Bobluxob");
        Debug.Log("Bobluxob scene built.");
    }

    // ---------------------------------------------------------------
    // MINEBLOXX
    // ---------------------------------------------------------------
    static void BuildMineBloxx()
    {
        var scene = NewScene("MineBloxx");

        // Camera (first-person)
        GameObject camGO = new GameObject("PlayerCamera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.backgroundColor = new Color(0.53f,0.81f,0.98f);
        cam.transform.position = new Vector3(0, 1.6f, 0);
        camGO.AddComponent<AudioListener>();

        // Directional light
        GameObject lightGO = new GameObject("Sun");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.eulerAngles = new Vector3(50,-30,0);

        // Player (CharacterController)
        GameObject playerGO = new GameObject("Player");
        playerGO.transform.position = new Vector3(0, 1.5f, 0);
        CharacterController cc = playerGO.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.center = new Vector3(0, 0.9f, 0);
        cam.transform.SetParent(playerGO.transform);
        cam.transform.localPosition = new Vector3(0, 1.6f, 0);

        // Canvas
        Canvas canvas = AddCanvas();
        Transform ct = canvas.transform;

        // Crosshair
        AddText(ct, "Crosshair", "+", 40, Color.white,
            TextAnchor.MiddleCenter,
            new Vector2(0.45f,0.45f), new Vector2(0.55f,0.55f), Vector2.zero, Vector2.zero);

        // Hotbar (9 slots)
        Image[]  slots     = new Image[9];
        Text[]   slotNames = new Text[9];
        for (int i = 0; i < 9; i++)
        {
            float left = 0.025f + i * 0.107f;
            GameObject slotGO = AddPanel(ct, "Slot_" + i, new Color(0.2f,0.2f,0.2f,0.7f),
                new Vector2(left, 0.01f), new Vector2(left + 0.10f, 0.12f),
                new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);
            slots[i] = slotGO.GetComponent<Image>();
            slotNames[i] = AddText(slotGO.transform, "SlotName", "", 12, Color.white,
                TextAnchor.LowerCenter,
                new Vector2(0,0), new Vector2(1,0.4f), Vector2.zero, Vector2.zero);
        }

        // Health & Hunger bars
        GameObject healthBG = AddPanel(ct, "HealthBG", new Color(0.3f,0.0f,0.0f),
            new Vector2(0.02f,0.13f), new Vector2(0.32f,0.18f), new Vector2(0,0.5f), Vector2.zero, Vector2.zero);
        GameObject healthFill = AddPanel(healthBG.transform, "HealthFill", new Color(0.9f,0.1f,0.1f),
            Vector2.zero, Vector2.one, new Vector2(0,0.5f), Vector2.zero, Vector2.zero);
        Image healthImg = healthFill.GetComponent<Image>();
        healthImg.type = Image.Type.Filled;
        healthImg.fillMethod = Image.FillMethod.Horizontal;

        GameObject hungerBG = AddPanel(ct, "HungerBG", new Color(0.3f,0.2f,0.0f),
            new Vector2(0.35f,0.13f), new Vector2(0.65f,0.18f), new Vector2(0,0.5f), Vector2.zero, Vector2.zero);
        GameObject hungerFill = AddPanel(hungerBG.transform, "HungerFill", new Color(0.9f,0.6f,0.1f),
            Vector2.zero, Vector2.one, new Vector2(0,0.5f), Vector2.zero, Vector2.zero);
        Image hungerImg = hungerFill.GetComponent<Image>();
        hungerImg.type = Image.Type.Filled;
        hungerImg.fillMethod = Image.FillMethod.Horizontal;

        Text healthTxt = AddText(ct, "HealthTxt", "❤ 20", 18, Color.white, TextAnchor.MiddleLeft,
            new Vector2(0.02f,0.18f), new Vector2(0.3f,0.23f), Vector2.zero, Vector2.zero);
        Text hungerTxt = AddText(ct, "HungerTxt", "🍗 20", 18, Color.white, TextAnchor.MiddleLeft,
            new Vector2(0.35f,0.18f), new Vector2(0.63f,0.23f), Vector2.zero, Vector2.zero);

        // Jump button (mobile)
        Button jumpBtn = AddButton(ct, "JumpButton", "Jump",
            new Color(0.2f,0.5f,0.9f,0.8f), Color.white, 24,
            new Vector2(0.78f,0.14f), new Vector2(0.98f,0.26f), Vector2.zero, Vector2.zero);

        BuildInGameHUD(ct);

        // Wire MineBloxx script
        MineBloxx mb = playerGO.AddComponent<MineBloxx>();
        mb.playerController = cc;
        mb.playerCamera     = cam.transform;
        mb.hotbarSlots      = slots;
        mb.hotbarNames      = slotNames;
        mb.healthBar        = healthImg;
        mb.hungerBar        = hungerImg;
        mb.healthText       = healthTxt;
        mb.hungerText       = hungerTxt;
        mb.jumpButton       = jumpBtn;

        SaveScene(scene, "MineBloxx");
        Debug.Log("MineBloxx scene built.");
    }

    // ---------------------------------------------------------------
    // VEHICLE SIMULATOR
    // ---------------------------------------------------------------
    static void BuildVehicleSimulator()
    {
        var scene = NewScene("VehicleSimulator");

        // Camera
        GameObject camGO = new GameObject("Main Camera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.backgroundColor = new Color(0.53f,0.81f,0.98f);
        cam.transform.position = new Vector3(0, 8, -15);
        cam.transform.eulerAngles = new Vector3(20, 0, 0);
        camGO.AddComponent<AudioListener>();

        // Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(20, 1, 20);
        ground.GetComponent<Renderer>().sharedMaterial.color = new Color(0.35f,0.65f,0.35f);

        // Directional light
        GameObject lightGO = new GameObject("Sun");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.eulerAngles = new Vector3(50,-30,0);

        // Vehicle spawn point
        GameObject spawnPt = new GameObject("VehicleSpawnPoint");
        spawnPt.transform.position = new Vector3(0, 2, 0);

        // Canvas
        Canvas canvas = AddCanvas();
        Transform ct = canvas.transform;

        // ---- Main Menu Panel ----
        GameObject mainPanel = AddPanel(ct, "MainMenuPanel", new Color(0.1f,0.1f,0.15f,0.96f),
            new Vector2(0.1f,0.15f), new Vector2(0.9f,0.85f), new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);

        AddText(mainPanel.transform, "Title", "Vehicle Simulator", 36, Color.white,
            TextAnchor.UpperCenter,
            new Vector2(0,0.82f), new Vector2(1,0.97f), Vector2.zero, Vector2.zero);

        Button vehicleBtn = AddButton(mainPanel.transform, "VehicleButton", "🚗  Select Vehicle",
            new Color(0.2f,0.4f,0.8f), Color.white, 28,
            new Vector2(0.1f,0.57f), new Vector2(0.9f,0.74f), Vector2.zero, Vector2.zero);

        Button mapBtn = AddButton(mainPanel.transform, "MapButton", "🗺  Select Map",
            new Color(0.1f,0.55f,0.2f), Color.white, 28,
            new Vector2(0.1f,0.36f), new Vector2(0.9f,0.53f), Vector2.zero, Vector2.zero);

        Button startBtn = AddButton(mainPanel.transform, "StartButton", "▶  Start!",
            new Color(0.8f,0.3f,0.1f), Color.white, 32,
            new Vector2(0.15f,0.08f), new Vector2(0.85f,0.28f), Vector2.zero, Vector2.zero);

        AddText(mainPanel.transform, "SelectedMapText", "Map: Big City", 20, Color.gray,
            TextAnchor.MiddleCenter,
            new Vector2(0.1f,0.29f), new Vector2(0.9f,0.36f), Vector2.zero, Vector2.zero);

        // ---- Vehicle Category Panel ----
        GameObject catPanel = AddPanel(ct, "VehicleCategoryPanel", new Color(0.08f,0.08f,0.14f,0.97f),
            new Vector2(0.05f,0.05f), new Vector2(0.95f,0.95f), new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);
        catPanel.SetActive(false);

        AddText(catPanel.transform, "Title", "Select Category", 30, Color.white,
            TextAnchor.UpperCenter,
            new Vector2(0,0.86f), new Vector2(1,0.97f), Vector2.zero, Vector2.zero);

        string[] catNames = { "✈ Planes", "🚁 Helis/Quads", "🚗 Cars", "⛵ Boats" };
        Color[]  catCols  = {
            new Color(0.3f,0.5f,0.9f), new Color(0.6f,0.2f,0.8f),
            new Color(0.8f,0.3f,0.1f), new Color(0.1f,0.5f,0.8f)
        };
        Button[] catBtns = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            float bot = 0.60f - i * 0.17f;
            catBtns[i] = AddButton(catPanel.transform, "Cat_" + i, catNames[i],
                catCols[i], Color.white, 26,
                new Vector2(0.05f, bot), new Vector2(0.95f, bot + 0.15f), Vector2.zero, Vector2.zero);
        }
        AddButton(catPanel.transform, "SaveGoBack", "✔ Save & Go Back",
            new Color(0.2f,0.65f,0.2f), Color.white, 24,
            new Vector2(0.1f,0.02f), new Vector2(0.9f,0.14f), Vector2.zero, Vector2.zero);

        // ---- Vehicle List Panel ----
        GameObject vListPanel = AddPanel(ct, "VehicleListPanel", new Color(0.08f,0.08f,0.14f,0.97f),
            new Vector2(0.05f,0.05f), new Vector2(0.95f,0.95f), new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);
        vListPanel.SetActive(false);

        AddText(vListPanel.transform, "Title", "Select Vehicle", 30, Color.white,
            TextAnchor.UpperCenter,
            new Vector2(0,0.88f), new Vector2(1,0.97f), Vector2.zero, Vector2.zero);

        // Scroll area for vehicle list
        GameObject scrollGO = new GameObject("ScrollArea");
        scrollGO.transform.SetParent(vListPanel.transform, false);
        RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.02f,0.05f);
        scrollRT.anchorMax = new Vector2(0.98f,0.87f);
        scrollRT.sizeDelta = Vector2.zero;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollGO.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0,1);
        contentRT.anchorMax = new Vector2(1,1);
        contentRT.pivot = new Vector2(0.5f,1);
        contentRT.sizeDelta = new Vector2(0, 600);
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(8,8,8,8);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Prefab-like vehicle entry (we create a simple template GO)
        GameObject vehicleEntryPrefab = new GameObject("VehicleEntryPrefab");
        vehicleEntryPrefab.transform.SetParent(content.transform, false);
        RectTransform vert = vehicleEntryPrefab.AddComponent<RectTransform>();
        vert.sizeDelta = new Vector2(0, 80);
        vehicleEntryPrefab.AddComponent<Image>().color = new Color(0.2f,0.22f,0.3f);
        vehicleEntryPrefab.AddComponent<Button>();
        AddText(vehicleEntryPrefab.transform, "VehicleText", "Vehicle Name\nDescription", 18, Color.white,
            TextAnchor.MiddleLeft, new Vector2(0.02f,0), new Vector2(0.98f,1), Vector2.zero, Vector2.zero);
        vehicleEntryPrefab.SetActive(false); // hide template

        // ---- Map Select Panel ----
        GameObject mapPanel = AddPanel(ct, "MapSelectPanel", new Color(0.08f,0.08f,0.14f,0.97f),
            new Vector2(0.05f,0.05f), new Vector2(0.95f,0.95f), new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);
        mapPanel.SetActive(false);

        AddText(mapPanel.transform, "Title", "Select Map", 32, Color.white,
            TextAnchor.UpperCenter,
            new Vector2(0,0.86f), new Vector2(1,0.97f), Vector2.zero, Vector2.zero);

        string[] mapNames = { "Big City","Water For Boat","Lots of Roads","Plain Take Off Area","Jump Paradise" };
        Button[] mapBtns = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            float bot = 0.70f - i * 0.14f;
            mapBtns[i] = AddButton(mapPanel.transform, "Map_" + i, mapNames[i],
                new Color(0.25f,0.4f,0.6f), Color.white, 24,
                new Vector2(0.05f, bot), new Vector2(0.95f, bot + 0.12f), Vector2.zero, Vector2.zero);
        }
        AddButton(mapPanel.transform, "MapBack", "← Back",
            new Color(0.4f,0.4f,0.4f), Color.white, 24,
            new Vector2(0.1f,0.02f), new Vector2(0.9f,0.12f), Vector2.zero, Vector2.zero);

        // ---- Gameplay Panel ----
        GameObject gpPanel = new GameObject("GameplayPanel");
        gpPanel.transform.SetParent(ct, false);
        RectTransform gpRT = gpPanel.AddComponent<RectTransform>();
        gpRT.anchorMin = Vector2.zero; gpRT.anchorMax = Vector2.one;
        gpRT.sizeDelta = Vector2.zero;
        gpPanel.SetActive(false);

        Text vehicleNameHUD = AddText(gpPanel.transform, "VehicleNameHUD", "Vehicle", 26, Color.white,
            TextAnchor.UpperLeft,
            new Vector2(0.02f,0.88f), new Vector2(0.5f,0.97f), Vector2.zero, Vector2.zero);
        Text speedTxt = AddText(gpPanel.transform, "SpeedText", "Speed: 0 MPH", 24, Color.white,
            TextAnchor.UpperLeft,
            new Vector2(0.02f,0.80f), new Vector2(0.5f,0.88f), Vector2.zero, Vector2.zero);
        AddText(gpPanel.transform, "SelectedMapText", "Map: Big City", 20, Color.gray,
            TextAnchor.UpperRight,
            new Vector2(0.5f,0.80f), new Vector2(0.98f,0.88f), Vector2.zero, Vector2.zero);

        // Joystick
        GameObject joystickOuter = AddPanel(gpPanel.transform, "JoystickOuter", new Color(0,0,0,0.3f),
            new Vector2(0.02f,0.05f), new Vector2(0.02f,0.05f), new Vector2(0.5f,0.5f),
            new Vector2(140,140), new Vector2(80, 80));
        GameObject joystickInner = AddPanel(joystickOuter.transform, "JoystickInner", new Color(1,1,1,0.6f),
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(60,60), Vector2.zero);

        Button throttleUp = AddButton(gpPanel.transform, "ThrottleUp", "▲",
            new Color(0.2f,0.7f,0.2f,0.8f), Color.white, 28,
            new Vector2(0.78f,0.20f), new Vector2(0.95f,0.33f), Vector2.zero, Vector2.zero);
        Button throttleDown = AddButton(gpPanel.transform, "ThrottleDown", "▼",
            new Color(0.8f,0.2f,0.2f,0.8f), Color.white, 28,
            new Vector2(0.78f,0.06f), new Vector2(0.95f,0.19f), Vector2.zero, Vector2.zero);

        BuildInGameHUD(gpPanel.transform);

        // Wire VehicleSimulator script
        GameObject vsGO = new GameObject("VehicleSimulatorController");
        VehicleSimulator vs = vsGO.AddComponent<VehicleSimulator>();
        vs.mainMenuPanel         = mainPanel;
        vs.vehicleCategoryPanel  = catPanel;
        vs.vehicleListPanel      = vListPanel;
        vs.mapSelectPanel        = mapPanel;
        vs.gameplayPanel         = gpPanel;
        vs.vehicleButton         = vehicleBtn;
        vs.mapButton             = mapBtn;
        vs.startButton           = startBtn;
        vs.planesButton          = catBtns[0];
        vs.helisButton           = catBtns[1];
        vs.carsButton            = catBtns[2];
        vs.boatsButton           = catBtns[3];
        vs.vehicleListContent    = content.transform;
        vs.vehicleEntryPrefab    = vehicleEntryPrefab;
        vs.mapButtons            = mapBtns;
        vs.vehicleNameHUD        = vehicleNameHUD;
        vs.speedText             = speedTxt;
        vs.joystickOuter         = joystickOuter.GetComponent<RectTransform>();
        vs.joystickInner         = joystickInner.GetComponent<RectTransform>();
        vs.throttleUpButton      = throttleUp;
        vs.throttleDownButton    = throttleDown;
        vs.vehicleSpawnPoint     = spawnPt.transform;

        SaveScene(scene, "VehicleSimulator");
        Debug.Log("VehicleSimulator scene built.");
    }

    // ---------------------------------------------------------------
    // SHARED: In-Game HUD (Settings / Save / Exit bar)
    // ---------------------------------------------------------------
    static void BuildInGameHUD(Transform canvasRoot)
    {
        GameObject hudPanel = AddPanel(canvasRoot, "InGameHUDPanel", new Color(0,0,0,0.55f),
            new Vector2(0,1f), new Vector2(1f,1f), new Vector2(0.5f,1f),
            new Vector2(0, 80), Vector2.zero);

        AddButton(hudPanel.transform, "SettingsBtn", "⚙",
            new Color(0.3f,0.3f,0.4f), Color.white, 28,
            new Vector2(0.01f,0.1f), new Vector2(0.14f,0.9f), Vector2.zero, Vector2.zero);

        AddButton(hudPanel.transform, "SaveBtn", "💾 Save",
            new Color(0.1f,0.5f,0.2f), Color.white, 22,
            new Vector2(0.38f,0.1f), new Vector2(0.62f,0.9f), Vector2.zero, Vector2.zero);

        AddButton(hudPanel.transform, "ExitBtn", "✖ Exit",
            new Color(0.7f,0.15f,0.1f), Color.white, 22,
            new Vector2(0.64f,0.1f), new Vector2(0.88f,0.9f), Vector2.zero, Vector2.zero);

        // Settings popup
        GameObject settingsPopup = AddPanel(canvasRoot, "InGameSettingsPopup", new Color(0.1f,0.1f,0.2f,0.97f),
            new Vector2(0.1f,0.3f), new Vector2(0.9f,0.85f), new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);
        settingsPopup.SetActive(false);

        AddText(settingsPopup.transform, "Title", "Settings", 32, Color.white,
            TextAnchor.UpperCenter,
            new Vector2(0,0.82f), new Vector2(1,0.97f), Vector2.zero, Vector2.zero);

        AddText(settingsPopup.transform, "MusicLbl", "Music Volume", 22, Color.white,
            TextAnchor.MiddleLeft,
            new Vector2(0.05f,0.62f), new Vector2(0.45f,0.72f), Vector2.zero, Vector2.zero);

        AddText(settingsPopup.transform, "SFXLbl", "SFX Volume", 22, Color.white,
            TextAnchor.MiddleLeft,
            new Vector2(0.05f,0.48f), new Vector2(0.45f,0.58f), Vector2.zero, Vector2.zero);

        AddButton(settingsPopup.transform, "ClosePopup", "Close",
            new Color(0.7f,0.15f,0.1f), Color.white, 24,
            new Vector2(0.25f,0.05f), new Vector2(0.75f,0.18f), Vector2.zero, Vector2.zero);

        // InGameHUD self-wires at runtime via GameObject.Find
        hudPanel.AddComponent<InGameHUD>();
    }

    // ---------------------------------------------------------------
    // BUILD SETTINGS
    // ---------------------------------------------------------------
    static void AddScenesToBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene(ScenesPath + "MainMenu.unity",        true),
            new EditorBuildSettingsScene(ScenesPath + "CookieClicker.unity",   true),
            new EditorBuildSettingsScene(ScenesPath + "Game2040.unity",        true),
            new EditorBuildSettingsScene(ScenesPath + "Bobluxob.unity",        true),
            new EditorBuildSettingsScene(ScenesPath + "MineBloxx.unity",       true),
            new EditorBuildSettingsScene(ScenesPath + "VehicleSimulator.unity",true),
            new EditorBuildSettingsScene(ScenesPath + "PaperSnake.unity",      true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build settings updated with all 7 scenes.");
    }
}
#endif
