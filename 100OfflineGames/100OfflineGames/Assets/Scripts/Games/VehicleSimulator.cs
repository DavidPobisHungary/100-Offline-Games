using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Vehicle Simulator
/// Menu: Vehicle picker (Planes / Helis+Quads / Cars / Boats) + Map picker
/// Gameplay: physics Rigidbody vehicle with on-screen joystick controls
/// </summary>
public class VehicleSimulator : MonoBehaviour
{
    // ---- UI Panels ----
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject vehicleCategoryPanel;
    public GameObject vehicleListPanel;
    public GameObject mapSelectPanel;
    public GameObject gameplayPanel;

    [Header("Main Menu Buttons")]
    public Button vehicleButton;
    public Button mapButton;
    public Button startButton;

    [Header("Category Buttons")]
    public Button planesButton;
    public Button helisButton;
    public Button carsButton;
    public Button boatsButton;
    public Button saveAndGoBackButton;

    [Header("Vehicle List")]
    public Transform vehicleListContent;
    public GameObject vehicleEntryPrefab; // Button + Name + Description texts

    [Header("Map Select")]
    public Button[] mapButtons;       // 5 map buttons
    public Button mapBackButton;

    [Header("Gameplay HUD")]
    public Text vehicleNameHUD;
    public Text speedText;
    public Text selectedMapText;

    // ---- Joystick / Controls ----
    [Header("Joystick")]
    public RectTransform joystickOuter;
    public RectTransform joystickInner;
    public Button throttleUpButton;
    public Button throttleDownButton;

    // ---- Vehicle prefabs (assign in Inspector or create via code) ----
    [Header("Vehicle Root (spawned)")]
    public Transform vehicleSpawnPoint;

    // ---- State ----
    private string selectedVehicleName = "Honda Civic 8 Gen 1.8 Turbo Diezel";
    private float  selectedMaxSpeed    = 1000f;
    private bool   isAerial            = false;  // plane/heli = true
    private string selectedMap         = "Big City";

    private Rigidbody vehicleRb;
    private GameObject spawnedVehicle;

    private float thrust    = 0f;
    private float steer     = 0f;
    private float lift      = 0f;

    // -------- Vehicle Data --------
    private struct VehicleData
    {
        public string name;
        public string description;
        public float  maxSpeed;
        public bool   aerial;
    }

    private static readonly VehicleData[] allVehicles = {
        // Planes
        new VehicleData{ name="Advantage",         description="Scale Electric high wing aileron trainer. 40+ MPH",          maxSpeed=40f,    aerial=true },
        new VehicleData{ name="A-10 Warthog",       description="Scale Model of US Army ground attack plane. Fires fireballs!",maxSpeed=500f,   aerial=true },
        new VehicleData{ name="A380 EPO EDF",        description="Large Electric Ducted Fan airliner model.",                  maxSpeed=200f,   aerial=true },
        new VehicleData{ name="Edge 540 25%",        description="Nice 3D/scale plane to advance you as 3D Pilot.",            maxSpeed=120f,   aerial=true },
        // Helis/Quads
        new VehicleData{ name="Model CX",            description="Sport Coaxial Electric Helicopter.",                         maxSpeed=60f,    aerial=true },
        new VehicleData{ name="C+Gamer F929 Drone81",description="Quadcopter for gamers. 100+ MPH!",                          maxSpeed=100f,   aerial=true },
        new VehicleData{ name="Bell 47",             description="Big Boring Scale Model Bell 47. 60+ MPH.",                   maxSpeed=60f,    aerial=true },
        new VehicleData{ name="Blast 450 3D",        description="Popular 3D Collective Helicopter.",                          maxSpeed=80f,    aerial=true },
        // Cars
        new VehicleData{ name="Honda Civic 8 Gen 1.8 Turbo Diezel", description="Nice and Fast Honda. 1000+ MPH TURBO DIEZEL!", maxSpeed=1000f, aerial=false },
        new VehicleData{ name="2015 Ferrari LaFerrari",description="SUPER DUPER FAST. 10000+ MPH IN 5 MINUTES. DRIFT KING!", maxSpeed=10000f,aerial=false },
        new VehicleData{ name="Little Car Suzuki",   description="Just That Little Suzuki Swift.",                             maxSpeed=120f,   aerial=false },
        new VehicleData{ name="BMW M2 Coupe",        description="Nice BMW M2 Coupe. Fast. Looks Good. 70 MPH.",               maxSpeed=70f,    aerial=false },
        // Boats
        new VehicleData{ name="xGOWup T.R.I RFN10450",description="A Boat That Goes Up To 51 MPH.",                          maxSpeed=51f,    aerial=false },
    };

    private static readonly string[] mapNames = {
        "Big City",
        "Water For Boat",
        "Lots of Roads",
        "Plain Take Off Area",
        "Jump Paradise"
    };

    void Start()
    {
        ShowPanel(mainMenuPanel);

        vehicleButton.onClick.AddListener(() => ShowPanel(vehicleCategoryPanel));
        mapButton.onClick.AddListener(() => ShowMapPanel());
        startButton.onClick.AddListener(StartDriving);

        planesButton.onClick.AddListener(() => ShowVehicleList(0, 3));
        helisButton.onClick.AddListener(() => ShowVehicleList(4, 7));
        carsButton.onClick.AddListener(() => ShowVehicleList(8, 11));
        boatsButton.onClick.AddListener(() => ShowVehicleList(12, 12));
        saveAndGoBackButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));

        for (int i = 0; i < mapButtons.Length; i++)
        {
            int idx = i;
            if (mapButtons[i] != null)
            {
                mapButtons[i].GetComponentInChildren<Text>().text = mapNames[i];
                mapButtons[i].onClick.AddListener(() => {
                    selectedMap = mapNames[idx];
                    if (selectedMapText) selectedMapText.text = "Map: " + selectedMap;
                    ShowPanel(mainMenuPanel);
                });
            }
        }
        // Self-wire refs not assigned by SceneBuilder
        if (saveAndGoBackButton == null)
        {
            GameObject go = GameObject.Find("SaveGoBack");
            if (go != null) saveAndGoBackButton = go.GetComponent<Button>();
        }
        if (saveAndGoBackButton) saveAndGoBackButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));

        if (mapBackButton == null)
        {
            GameObject go = GameObject.Find("MapBack");
            if (go != null) mapBackButton = go.GetComponent<Button>();
        }
        if (mapBackButton) mapBackButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));

        if (selectedMapText == null)
        {
            GameObject go = GameObject.Find("SelectedMapText");
            if (go != null) selectedMapText = go.GetComponent<Text>();
        }
    }

    void ShowPanel(GameObject panel)
    {
        mainMenuPanel.SetActive(panel == mainMenuPanel);
        vehicleCategoryPanel.SetActive(panel == vehicleCategoryPanel);
        vehicleListPanel.SetActive(panel == vehicleListPanel);
        mapSelectPanel.SetActive(panel == mapSelectPanel);
        gameplayPanel.SetActive(panel == gameplayPanel);
    }

    void ShowMapPanel() { ShowPanel(mapSelectPanel); }

    void ShowVehicleList(int from, int to)
    {
        ShowPanel(vehicleListPanel);
        foreach (Transform child in vehicleListContent) Destroy(child.gameObject);

        for (int i = from; i <= to; i++)
        {
            int idx = i;
            GameObject entry = Instantiate(vehicleEntryPrefab, vehicleListContent);
            Text[] texts = entry.GetComponentsInChildren<Text>();
            if (texts.Length >= 2) { texts[0].text = allVehicles[i].name; texts[1].text = allVehicles[i].description; }
            else if (texts.Length == 1) texts[0].text = allVehicles[i].name + "\n" + allVehicles[i].description;

            Button btn = entry.GetComponent<Button>();
            if (btn == null) btn = entry.GetComponentInChildren<Button>();
            if (btn != null) btn.onClick.AddListener(() => {
                selectedVehicleName = allVehicles[idx].name;
                selectedMaxSpeed    = allVehicles[idx].maxSpeed;
                isAerial            = allVehicles[idx].aerial;
                ShowPanel(mainMenuPanel);
            });
        }
    }

    void StartDriving()
    {
        ShowPanel(gameplayPanel);
        if (vehicleNameHUD) vehicleNameHUD.text = selectedVehicleName;
        if (selectedMapText) selectedMapText.text = "Map: " + selectedMap;
        SpawnVehicle();
        SetupJoystick();
    }

    void SpawnVehicle()
    {
        if (spawnedVehicle != null) Destroy(spawnedVehicle);

        spawnedVehicle = new GameObject("Vehicle_" + selectedVehicleName);

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(spawnedVehicle.transform);
        body.transform.localScale = isAerial ? new Vector3(3f, 0.5f, 1.5f) : new Vector3(2f, 0.8f, 1f);
        body.transform.localPosition = Vector3.zero;

        // Colour by type
        Renderer r = body.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Diffuse"));
        r.material.color = isAerial ? Color.gray : Color.blue;

        // Physics
        vehicleRb = spawnedVehicle.AddComponent<Rigidbody>();
        vehicleRb.mass = isAerial ? 2f : 10f;
        vehicleRb.drag = isAerial ? 0.5f : 1f;
        vehicleRb.angularDrag = isAerial ? 2f : 3f;

        if (!isAerial) vehicleRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        Vector3 spawnPos = vehicleSpawnPoint != null ? vehicleSpawnPoint.position : Vector3.up * 2f;
        spawnedVehicle.transform.position = spawnPos;
    }

    void SetupJoystick()
    {
        if (throttleUpButton)   throttleUpButton.onClick.AddListener(() => thrust = 1f);
        if (throttleDownButton) throttleDownButton.onClick.AddListener(() => thrust = -1f);
    }

    void Update()
    {
        if (!gameplayPanel.activeSelf) return;
        HandleJoystickInput();
        DriveVehicle();
        UpdateSpeedText();
    }

    void HandleJoystickInput()
    {
        // Keyboard fallback
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
        {
            steer  = h;
            thrust = v;
        }

        // Touch joystick
        if (Input.touchCount > 0 && joystickOuter != null)
        {
            Touch t = Input.GetTouch(0);
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickOuter, t.position, null, out localPos);

            float radius = joystickOuter.sizeDelta.x * 0.5f;
            localPos = Vector2.ClampMagnitude(localPos, radius);
            if (joystickInner) joystickInner.anchoredPosition = localPos;

            steer  = localPos.x / radius;
            lift   = localPos.y / radius;
            if (!isAerial) thrust = localPos.y / radius;

            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                if (joystickInner) joystickInner.anchoredPosition = Vector2.zero;
                steer = 0f; lift = 0f;
            }
        }
    }

    void DriveVehicle()
    {
        if (vehicleRb == null) return;

        float speed = selectedMaxSpeed * 0.1f; // scale down for Unity units

        if (isAerial)
        {
            vehicleRb.AddForce(spawnedVehicle.transform.forward * thrust * speed);
            vehicleRb.AddForce(Vector3.up * lift * speed * 0.5f);
            vehicleRb.AddTorque(Vector3.up * steer * 30f);
            vehicleRb.AddForce(Vector3.up * 9.8f * vehicleRb.mass * 0.95f); // near-hover
        }
        else
        {
            vehicleRb.AddForce(spawnedVehicle.transform.forward * thrust * speed);
            vehicleRb.AddTorque(Vector3.up * steer * 20f);
        }
    }

    void UpdateSpeedText()
    {
        if (vehicleRb == null || speedText == null) return;
        float mph = vehicleRb.velocity.magnitude * 2.237f;
        speedText.text = "Speed: " + mph.ToString("0") + " MPH";
    }
}
