using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// MineBloxx - Minecraft-inspired with furniture and vehicles.
/// Player spawns inside a furnished house and can go outside.
/// Features: block placing/breaking, hotbar, health/hunger bars,
/// openable windows/doors, furniture items, vehicles outside.
/// </summary>
public class MineBloxx : MonoBehaviour
{
    [Header("Player")]
    public CharacterController playerController;
    public Transform playerCamera;
    public float moveSpeed    = 5f;
    public float runSpeed     = 10f;
    public float gravity      = -20f;
    public float jumpHeight   = 1.5f;

    [Header("UI")]
    public Text  crosshairText;      // simple "+" text
    public Text  hotbarLabel;
    public Image[] hotbarSlots;      // 9 slots
    public Text[]  hotbarNames;
    public Image healthBar;
    public Image hungerBar;
    public Text  healthText;
    public Text  hungerText;
    public Button jumpButton;

    [Header("Block Placing")]
    public float reach = 5f;
    public Material blockMaterial;

    // ---- Player state ----
    private float health = 20f;
    private float hunger = 20f;
    private float hungerTimer = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private int selectedSlot = 0;

    // ---- Hotbar items ----
    private string[] hotbarItems = {
        "Grass Block","Stone Block","Wood Block","Glass Block","TV","Chair",
        "Pizza","iPhone4Block","Car"
    };

    // ---- Placed blocks ----
    private List<GameObject> placedBlocks = new List<GameObject>();
    private Dictionary<GameObject, bool> windowStates = new Dictionary<GameObject, bool>(); // true=open

    // ---- Touch ----
    private Vector2 touchStartLook;
    private float rotX, rotY;

    void Start()
    {
        BuildHouse();
        SpawnVehiclesOutside();

        if (jumpButton) jumpButton.onClick.AddListener(TryJump);
        RefreshHotbarUI();

        // Lock/hide cursor on desktop
#if UNITY_EDITOR || UNITY_STANDALONE
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
    }

    void BuildHouse()
    {
        // === FLOOR ===
        SpawnBlock("Floor", new Vector3(0, 0, 0), new Vector3(12, 0.3f, 10), new Color(0.8f, 0.7f, 0.5f));

        // === WALLS ===
        SpawnBlock("WallBack",   new Vector3(0,  2.5f,  5),   new Vector3(12, 5, 0.3f), new Color(0.9f, 0.85f, 0.75f));
        SpawnBlock("WallFront",  new Vector3(0,  2.5f, -5),   new Vector3(12, 5, 0.3f), new Color(0.9f, 0.85f, 0.75f));
        SpawnBlock("WallLeft",   new Vector3(-6, 2.5f,  0),   new Vector3(0.3f, 5, 10), new Color(0.9f, 0.85f, 0.75f));
        SpawnBlock("WallRight",  new Vector3( 6, 2.5f,  0),   new Vector3(0.3f, 5, 10), new Color(0.9f, 0.85f, 0.75f));

        // === CEILING ===
        SpawnBlock("Ceiling",    new Vector3(0,  5.1f,  0),   new Vector3(12, 0.3f, 10), new Color(0.85f, 0.8f, 0.7f));

        // === FRONT DOOR (openable = destructible for simplicity) ===
        GameObject door = SpawnBlock("FrontDoor", new Vector3(0, 1.5f, -5), new Vector3(2, 3, 0.2f), new Color(0.5f, 0.3f, 0.1f));
        door.tag = "Door";
        door.AddComponent<Button>();   // handled by raycast

        // === WINDOW (openable) ===
        GameObject win = SpawnBlock("Window", new Vector3(3, 2.5f, -5), new Vector3(2, 2, 0.15f), new Color(0.7f, 0.9f, 1.0f, 0.5f));
        win.tag = "Window";
        windowStates[win] = false;

        // === FURNITURE ===

        // Couch (brown)
        SpawnBlock("Couch_Seat",  new Vector3(-3, 0.5f, 3), new Vector3(3, 0.6f, 1.2f), new Color(0.5f,0.25f,0.1f));
        SpawnBlock("Couch_Back",  new Vector3(-3, 1.2f, 3.6f), new Vector3(3, 1.0f, 0.3f), new Color(0.5f,0.25f,0.1f));

        // TV (black screen, grey frame)
        SpawnBlock("TV_Frame",  new Vector3(-3, 2.5f, 4.5f), new Vector3(2.5f, 1.5f, 0.2f), Color.gray);
        SpawnBlock("TV_Screen", new Vector3(-3, 2.5f, 4.4f), new Vector3(2.2f, 1.2f, 0.05f), Color.black);

        // Desk + Chair + PC + Monitor
        SpawnBlock("Desk",       new Vector3(4,  0.8f, 3), new Vector3(2, 0.1f, 1), new Color(0.6f,0.4f,0.2f));
        SpawnBlock("DeskLegs",   new Vector3(4,  0.4f, 3), new Vector3(1.8f, 0.8f, 0.8f), new Color(0.5f,0.35f,0.15f));
        SpawnBlock("Chair_Seat", new Vector3(4,  0.6f, 2), new Vector3(0.8f,0.1f,0.8f), new Color(0.3f,0.3f,0.7f));
        SpawnBlock("Chair_Back", new Vector3(4,  1.1f, 2.4f), new Vector3(0.8f,1.0f,0.1f), new Color(0.3f,0.3f,0.7f));
        SpawnBlock("Monitor",    new Vector3(4,  1.4f, 3.4f), new Vector3(1.2f,0.8f,0.05f), Color.black);
        SpawnBlock("Keyboard",   new Vector3(4,  0.9f, 3.1f), new Vector3(0.8f,0.05f,0.3f), new Color(0.2f,0.2f,0.2f));
        SpawnBlock("PCTower",    new Vector3(3.2f,0.9f,3.3f), new Vector3(0.4f,0.8f,0.5f), new Color(0.15f,0.15f,0.15f));
        SpawnBlock("Laptop",     new Vector3(4.5f,0.9f,3.0f), new Vector3(0.7f,0.05f,0.5f), new Color(0.2f,0.2f,0.2f));

        // Kitchen area
        SpawnBlock("KitchenCounter",new Vector3(-4, 0.9f, -3), new Vector3(4, 0.1f, 1.5f), new Color(0.9f,0.9f,0.9f));
        SpawnBlock("KitchenBase",   new Vector3(-4, 0.4f, -3), new Vector3(4, 0.8f, 1.5f), new Color(0.8f,0.8f,0.8f));
        SpawnBlock("KitchenShelf",  new Vector3(-4, 2.5f, -4.2f), new Vector3(4, 0.3f, 0.4f), new Color(0.85f,0.85f,0.85f));
        SpawnBlock("Stove",         new Vector3(-2, 1.0f, -3), new Vector3(1.2f,0.2f,0.8f), Color.gray);
        SpawnBlock("StoveBurners",  new Vector3(-2, 1.1f, -3), new Vector3(0.3f,0.05f,0.3f), Color.black);
        SpawnBlock("Fridge",        new Vector3(-5, 1.5f, -3), new Vector3(0.9f,3.0f,0.9f), Color.white);

        // Headphones on desk
        SpawnBlock("Headphones", new Vector3(4.8f, 0.95f, 3.3f), new Vector3(0.4f,0.3f,0.15f), Color.black);

        // Pizza on counter
        SpawnBlock("Pizza", new Vector3(-3, 1.05f, -3), new Vector3(0.5f,0.05f,0.5f), new Color(1f,0.6f,0.1f));

        // Fan (small cylinder on desk)
        SpawnBlock("Fan", new Vector3(5f, 1.05f, 3.3f), new Vector3(0.3f,0.5f,0.1f), new Color(0.6f,0.6f,0.8f));

        // Plant
        SpawnBlock("PlantPot", new Vector3(5, 0.4f, -4), new Vector3(0.4f,0.5f,0.4f), new Color(0.6f,0.4f,0.2f));
        SpawnBlock("Plant",    new Vector3(5, 1.0f, -4), new Vector3(0.5f,0.8f,0.5f), new Color(0.1f,0.7f,0.1f));

        // iPhone 4 on table
        SpawnBlock("iPhone4", new Vector3(-2, 1.05f, -2.5f), new Vector3(0.15f,0.3f,0.05f), Color.black);

        // Spawn player inside house
        if (playerController) playerController.transform.position = new Vector3(0, 1.5f, 0);
    }


    GameObject SpawnBlock(string name, Vector3 pos, Vector3 size, Color col)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = size;
        var mat = new Material(Shader.Find("Diffuse"));
        mat.color = col;
        go.GetComponent<Renderer>().material = mat;
        return go;
    }

    void SpawnVehiclesOutside()
    {
        // Driveable Car (just a Rigidbody box outside the house)
        SpawnPhysicsVehicle("Car",     new Vector3(15, 0.6f, 0), new Vector3(2f,1f,4f), Color.red);
        SpawnPhysicsVehicle("Bicycle", new Vector3(12, 0.6f, 5), new Vector3(0.4f,1.2f,2f), Color.cyan);
        SpawnPhysicsVehicle("Bus",     new Vector3(20, 1.5f, -5), new Vector3(3f,3f,8f), new Color(1f,0.8f,0f));
        SpawnPhysicsVehicle("Truck",   new Vector3(25, 1.5f, 0), new Vector3(3f,3f,10f), new Color(0.3f,0.3f,0.8f));
        SpawnPhysicsVehicle("Skateboard", new Vector3(10, 0.2f, -5), new Vector3(0.8f,0.15f,2.5f), new Color(0.6f,0.4f,0.2f));

        // Ground outside
        SpawnBlock("OutdoorGround", new Vector3(20, -0.15f, 0), new Vector3(60, 0.3f, 60), new Color(0.35f,0.6f,0.35f));
    }

    void SpawnPhysicsVehicle(string name, Vector3 pos, Vector3 size, Color col)
    {
        GameObject go = SpawnBlock(name, pos, size, col);
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.mass = 10f;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleInteraction();
        HandleHotbarKeys();
        UpdateHunger();
        RefreshBars();
    }

    void HandleMovement()
    {
        if (playerController == null) return;

        isGrounded = playerController.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = playerCamera.transform.right * h + playerCamera.transform.forward * v;
        move.y = 0;
        playerController.Move(move.normalized * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        playerController.Move(velocity * Time.deltaTime);
    }

    void TryJump()
    {
        if (isGrounded) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void HandleLook()
    {
        // Mouse look on desktop
        float mouseX = Input.GetAxis("Mouse X") * 2f;
        float mouseY = Input.GetAxis("Mouse Y") * 2f;

        rotY += mouseX;
        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, -80f, 80f);

        if (playerController) playerController.transform.rotation = Quaternion.Euler(0, rotY, 0);
        if (playerCamera)     playerCamera.localRotation = Quaternion.Euler(rotX, 0, 0);
    }

    void HandleInteraction()
    {
        // Left click = break block / right click = place
        if (Input.GetMouseButtonDown(0)) BreakBlock();
        if (Input.GetMouseButtonDown(1)) PlaceBlock();
    }

    void BreakBlock()
    {
        RaycastHit hit;
        if (!Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, reach)) return;
        if (hit.collider.name.StartsWith("Floor") ||
            hit.collider.name.StartsWith("Ceiling") ||
            hit.collider.name.StartsWith("Wall")) return; // indestructible house shell

        // Open door/window instead of breaking
        if (hit.collider.CompareTag("Door"))   { ToggleDoor(hit.collider.gameObject);   return; }
        if (hit.collider.CompareTag("Window")) { ToggleWindow(hit.collider.gameObject); return; }

        Destroy(hit.collider.gameObject);
    }

    void PlaceBlock()
    {
        RaycastHit hit;
        if (!Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, reach)) return;

        Vector3 placePos = hit.point + hit.normal * 0.5f;
        placePos = new Vector3(Mathf.Round(placePos.x), Mathf.Round(placePos.y), Mathf.Round(placePos.z));

        Color blockCol = GetBlockColor(hotbarItems[selectedSlot]);
        GameObject newBlock = SpawnBlock(hotbarItems[selectedSlot], placePos, Vector3.one, blockCol);
        placedBlocks.Add(newBlock);
    }

    Color GetBlockColor(string item)
    {
        switch (item)
        {
            case "Grass Block":  return new Color(0.3f,0.7f,0.2f);
            case "Stone Block":  return Color.gray;
            case "Wood Block":   return new Color(0.6f,0.4f,0.2f);
            case "Glass Block":  return new Color(0.7f,0.9f,1f,0.5f);
            case "TV":           return Color.black;
            case "Chair":        return new Color(0.4f,0.3f,0.7f);
            case "Pizza":        return new Color(1f,0.6f,0.1f);
            case "iPhone4Block": return Color.black;
            case "Car":          return Color.red;
            default:             return Color.white;
        }
    }

    void ToggleDoor(GameObject door)
    {
        // Simple open = move sideways
        door.transform.position += door.transform.right * 1.5f;
    }

    void ToggleWindow(GameObject win)
    {
        if (!windowStates.ContainsKey(win)) return;
        bool open = windowStates[win];
        windowStates[win] = !open;
        win.SetActive(open); // open = invisible (removed pane)
    }

    void HandleHotbarKeys()
    {
        for (int i = 0; i < 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { selectedSlot = i; RefreshHotbarUI(); }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) { selectedSlot = (selectedSlot - 1 + 9) % 9; RefreshHotbarUI(); }
        if (scroll < 0f) { selectedSlot = (selectedSlot + 1) % 9;     RefreshHotbarUI(); }
    }

    void UpdateHunger()
    {
        hungerTimer += Time.deltaTime;
        if (hungerTimer >= 10f)
        {
            hungerTimer = 0f;
            hunger = Mathf.Max(0, hunger - 1);
            if (hunger <= 0) health = Mathf.Max(0, health - 1);
        }

        // Eating: press E when Pizza in hotbar
        if (Input.GetKeyDown(KeyCode.E) && hotbarItems[selectedSlot] == "Pizza")
            hunger = Mathf.Min(20f, hunger + 4f);
    }

    void RefreshBars()
    {
        if (healthBar) healthBar.fillAmount = health / 20f;
        if (hungerBar) hungerBar.fillAmount = hunger / 20f;
        if (healthText) healthText.text = "❤ " + (int)health;
        if (hungerText) hungerText.text = "🍗 " + (int)hunger;
    }

    void RefreshHotbarUI()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i]) hotbarSlots[i].color = i == selectedSlot ? Color.yellow : Color.white;
            if (hotbarNames != null && i < hotbarNames.Length && hotbarNames[i])
                hotbarNames[i].text = hotbarItems[i];
        }
    }
}
