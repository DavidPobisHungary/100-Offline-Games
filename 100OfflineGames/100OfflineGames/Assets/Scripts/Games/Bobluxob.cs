using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Bobluxob - Roblox-inspired game.
/// Player: classic noob avatar (colored blocks).
/// Items: Sword, RocketLauncher, Boombox, WallBuilder.
/// NPCs: walk, jump, pick up items, can be killed.
/// Ragdoll button in bottom-right corner.
/// Map: Crossroads-style flat area with paths.
/// </summary>
public class Bobluxob : MonoBehaviour
{
    [Header("Player")]
    public Transform playerTransform;
    public Rigidbody playerRb;
    public float moveSpeed = 8f;
    public float jumpForce = 350f;

    [Header("UI")]
    public Text hotbarText;
    public Button ragdollButton;
    public Button[] hotbarButtons; // 4 item slots
    public Text[] hotbarItemNames;
    public Text killFeedText;
    public RectTransform joystickOuter;
    public RectTransform joystickInner;
    public Button jumpButton;

    [Header("Prefabs / Parents")]
    public Transform npcParent;
    public Transform itemSpawnParent;

    // ---- Item system ----
    private enum Item { None, Sword, RocketLauncher, Boombox, WallBuilder }
    private Item[] hotbar = { Item.Sword, Item.RocketLauncher, Item.Boombox, Item.WallBuilder };
    private int selectedSlot = 0;

    // ---- Ragdoll ----
    private bool isRagdoll = false;
    private Rigidbody[] ragdollBodies;

    // ---- NPC ----
    private List<BobluxobNPC> npcs = new List<BobluxobNPC>();
    private const int NPC_COUNT = 6;

    // ---- Ground check ----
    private bool grounded = false;
    private Vector2 touchStart;

    // ---- Kill feed ----
    private float killFeedTimer = 0f;

    void Start()
    {
        BuildMap();
        BuildPlayerAvatar();
        SpawnItems();
        SpawnNPCs();

        ragdollButton.onClick.AddListener(ToggleRagdoll);
        jumpButton.onClick.AddListener(Jump);

        for (int i = 0; i < hotbarButtons.Length; i++)
        {
            int idx = i;
            hotbarButtons[i].onClick.AddListener(() => selectedSlot = idx);
            hotbarItemNames[i].text = hotbar[i].ToString();
        }

        RefreshHotbarUI();
    }

    void BuildMap()
    {
        // Ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10, 1, 10);
        ground.GetComponent<Renderer>().material.color = new Color(0.4f, 0.7f, 0.4f);

        // Crossroads-style roads (grey boxes)
        SpawnBox("Road_H", new Vector3(0, 0.01f, 0), new Vector3(100, 0.1f, 8), new Color(0.4f, 0.4f, 0.4f));
        SpawnBox("Road_V", new Vector3(0, 0.01f, 0), new Vector3(8, 0.1f, 100), new Color(0.4f, 0.4f, 0.4f));

        // Some walls / structures
        SpawnBox("Wall1", new Vector3(15, 2, 15), new Vector3(2, 4, 10), Color.gray);
        SpawnBox("Wall2", new Vector3(-15, 2, -15), new Vector3(10, 4, 2), Color.gray);
        SpawnBox("Pillar1", new Vector3(10, 3, 0), new Vector3(2, 6, 2), new Color(0.8f, 0.6f, 0.2f));
        SpawnBox("Pillar2", new Vector3(-10, 3, 0), new Vector3(2, 6, 2), new Color(0.8f, 0.6f, 0.2f));
    }

    void SpawnBox(string name, Vector3 pos, Vector3 size, Color col)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().material.color = col;
    }

    void BuildPlayerAvatar()
    {
        if (playerTransform == null) return;

        // Head (yellow)
        SpawnAvatarPart(playerTransform, "Head",  Vector3.up * 1.8f, new Vector3(0.8f,0.8f,0.8f), new Color(1f,0.85f,0.3f));
        // Torso (blue)
        SpawnAvatarPart(playerTransform, "Torso", Vector3.up * 0.9f, new Vector3(0.9f,1.0f,0.5f), new Color(0.2f,0.4f,1.0f));
        // Left Arm (blue)
        SpawnAvatarPart(playerTransform, "LA",    new Vector3(-0.7f,0.9f,0f), new Vector3(0.3f,1.0f,0.3f), new Color(0.2f,0.4f,1.0f));
        // Right Arm
        SpawnAvatarPart(playerTransform, "RA",    new Vector3( 0.7f,0.9f,0f), new Vector3(0.3f,1.0f,0.3f), new Color(0.2f,0.4f,1.0f));
        // Left Leg (green)
        SpawnAvatarPart(playerTransform, "LL",    new Vector3(-0.3f,0f,0f),   new Vector3(0.35f,0.9f,0.35f), new Color(0.1f,0.7f,0.2f));
        // Right Leg
        SpawnAvatarPart(playerTransform, "RL",    new Vector3( 0.3f,0f,0f),   new Vector3(0.35f,0.9f,0.35f), new Color(0.1f,0.7f,0.2f));
    }

    void SpawnAvatarPart(Transform parent, string partName, Vector3 localPos, Vector3 size, Color col)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(parent);
        part.transform.localPosition = localPos;
        part.transform.localScale = size;
        part.GetComponent<Renderer>().material.color = col;
    }

    void SpawnItems()
    {
        // Scatter pickup items on the map
        string[] itemNames = { "Sword", "RocketLauncher", "Boombox", "WallBuilder" };
        Vector3[] positions = {
            new Vector3(5, 0.5f, 5), new Vector3(-5, 0.5f, 5),
            new Vector3(5, 0.5f, -5), new Vector3(-5, 0.5f, -5)
        };

        for (int i = 0; i < itemNames.Length; i++)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            item.name = "Pickup_" + itemNames[i];
            item.transform.position = positions[i];
            item.transform.localScale = Vector3.one * 0.5f;
            item.GetComponent<Renderer>().material.color = Color.yellow;
            if (itemSpawnParent) item.transform.SetParent(itemSpawnParent);
        }
    }

    void SpawnNPCs()
    {
        for (int i = 0; i < NPC_COUNT; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-20f, 20f), 0.1f, Random.Range(-20f, 20f));
            GameObject npcGO = new GameObject("NPC_" + i);
            npcGO.transform.position = pos;
            if (npcParent) npcGO.transform.SetParent(npcParent);

            // NPC avatar
            BuildNPCAvatar(npcGO.transform);

            Rigidbody rb = npcGO.AddComponent<Rigidbody>();
            rb.freezeRotation = true;

            CapsuleCollider col = npcGO.AddComponent<CapsuleCollider>();
            col.height = 2.5f;
            col.center = Vector3.up * 1.25f;

            BobluxobNPC npc = npcGO.AddComponent<BobluxobNPC>();
            npc.playerTransform = playerTransform;
            npc.bobluxob = this;
            npcs.Add(npc);
        }
    }

    void BuildNPCAvatar(Transform parent)
    {
        // Simple noob: same as player but with random torso colour
        Color torsoCol = new Color(Random.value, Random.value, Random.value);
        SpawnAvatarPart(parent, "Head",  Vector3.up * 1.8f, new Vector3(0.8f,0.8f,0.8f), new Color(1f,0.85f,0.3f));
        SpawnAvatarPart(parent, "Torso", Vector3.up * 0.9f, new Vector3(0.9f,1.0f,0.5f), torsoCol);
        SpawnAvatarPart(parent, "LA",    new Vector3(-0.7f,0.9f,0f), new Vector3(0.3f,1.0f,0.3f), torsoCol);
        SpawnAvatarPart(parent, "RA",    new Vector3( 0.7f,0.9f,0f), new Vector3(0.3f,1.0f,0.3f), torsoCol);
        SpawnAvatarPart(parent, "LL",    new Vector3(-0.3f,0f,0f),   new Vector3(0.35f,0.9f,0.35f), new Color(0.1f,0.7f,0.2f));
        SpawnAvatarPart(parent, "RL",    new Vector3( 0.3f,0f,0f),   new Vector3(0.35f,0.9f,0.35f), new Color(0.1f,0.7f,0.2f));
    }

    void Update()
    {
        if (isRagdoll) return;

        HandleMovement();
        HandleUseItem();

        // Hotbar keyboard
        for (int i = 0; i < 4; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { selectedSlot = i; RefreshHotbarUI(); }

        // Kill feed fade
        if (killFeedTimer > 0)
        {
            killFeedTimer -= Time.deltaTime;
            if (killFeedTimer <= 0 && killFeedText) killFeedText.text = "";
        }

        // Ground check
        grounded = Physics.Raycast(playerTransform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);
    }

    void HandleMovement()
    {
        if (playerRb == null) return;

        Vector3 move = Vector3.zero;

        // Keyboard
        move.x = Input.GetAxis("Horizontal");
        move.z = Input.GetAxis("Vertical");

        // Touch joystick
        if (Input.touchCount > 0 && joystickOuter != null)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) touchStart = t.position;
            Vector2 delta = t.position - touchStart;
            float radius = joystickOuter.sizeDelta.x * 0.5f;
            delta = Vector2.ClampMagnitude(delta, radius);
            if (joystickInner) joystickInner.anchoredPosition = delta;
            move.x = delta.x / radius;
            move.z = delta.y / radius;
        }

        playerRb.MovePosition(playerTransform.position + move.normalized * moveSpeed * Time.deltaTime);

        if (move.sqrMagnitude > 0.01f)
            playerTransform.forward = Vector3.Lerp(playerTransform.forward, move.normalized, 10f * Time.deltaTime);
    }

    void Jump()
    {
        if (playerRb == null || !grounded) return;
        playerRb.AddForce(Vector3.up * jumpForce);
    }

    void HandleUseItem()
    {
        bool useTap = Input.GetMouseButtonDown(0);
        if (Input.touchCount > 1) useTap = true; // two-finger tap = use

        if (!useTap) return;

        Item current = hotbar[selectedSlot];
        switch (current)
        {
            case Item.Sword:        UseSword();        break;
            case Item.RocketLauncher: UseRocket();     break;
            case Item.Boombox:      UseBoombox();      break;
            case Item.WallBuilder:  UseWallBuilder();  break;
        }
    }

    void UseSword()
    {
        // Raycast in front, damage first NPC in range
        RaycastHit hit;
        if (Physics.Raycast(playerTransform.position + Vector3.up, playerTransform.forward, out hit, 3f))
        {
            BobluxobNPC npc = hit.collider.GetComponentInParent<BobluxobNPC>();
            if (npc != null) { npc.TakeDamage(50); ShowKillFeed("Sword hit " + npc.name + "!"); }
        }
    }

    void UseRocket()
    {
        // Spawn a physics rocket
        GameObject rocket = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rocket.name = "Rocket";
        rocket.transform.position = playerTransform.position + Vector3.up + playerTransform.forward;
        rocket.transform.localScale = Vector3.one * 0.3f;
        rocket.GetComponent<Renderer>().material.color = Color.red;
        Rigidbody rb = rocket.AddComponent<Rigidbody>();
        rb.velocity = playerTransform.forward * 30f;
        Destroy(rocket, 4f);

        // Simple explosion after delay
        BobluxobRocket br = rocket.AddComponent<BobluxobRocket>();
        br.bobluxob = this;
        br.npcs = npcs;
    }

    void UseBoombox()
    {
        // Plays a "boombox" — just visual effect placeholder
        ShowKillFeed("Boombox playing! (no audio in demo)");
    }

    void UseWallBuilder()
    {
        // Spawn a non-anchored wall (physics, not anchored)
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "BuiltWall";
        wall.transform.position = playerTransform.position + playerTransform.forward * 2f + Vector3.up;
        wall.transform.localScale = new Vector3(2f, 3f, 0.3f);
        wall.transform.rotation = playerTransform.rotation;
        wall.GetComponent<Renderer>().material.color = new Color(0.7f, 0.5f, 0.3f);
        Rigidbody wb = wall.AddComponent<Rigidbody>();
        wb.mass = 5f;
        // NOT kinematic = not anchored, can be knocked over
    }

    public void ToggleRagdoll()
    {
        isRagdoll = !isRagdoll;
        if (playerRb != null)
        {
            playerRb.isKinematic = !isRagdoll;
            if (isRagdoll) playerRb.AddForce(Random.insideUnitSphere * 500f);
        }
        if (ragdollButton)
            ragdollButton.GetComponentInChildren<Text>().text = isRagdoll ? "Stand Up" : "Ragdoll";
    }

    public void ShowKillFeed(string msg)
    {
        if (killFeedText) { killFeedText.text = msg; killFeedTimer = 3f; }
    }

    void RefreshHotbarUI()
    {
        for (int i = 0; i < hotbarButtons.Length; i++)
        {
            ColorBlock cb = hotbarButtons[i].colors;
            cb.normalColor = i == selectedSlot ? Color.yellow : Color.white;
            hotbarButtons[i].colors = cb;
        }
    }
}

/// <summary>Simple NPC behaviour.</summary>
public class BobluxobNPC : MonoBehaviour
{
    public Transform playerTransform;
    public Bobluxob bobluxob;

    private int health = 100;
    private Rigidbody rb;
    private float moveTimer;
    private Vector3 moveDir;
    private float jumpTimer;
    private bool alive = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        moveDir = Random.insideUnitSphere;
        moveDir.y = 0;
        moveDir.Normalize();
    }

    void Update()
    {
        if (!alive || rb == null) return;

        moveTimer -= Time.deltaTime;
        jumpTimer -= Time.deltaTime;

        if (moveTimer <= 0)
        {
            moveTimer = Random.Range(1f, 3f);
            // Wander or move toward player occasionally
            if (playerTransform != null && Random.value < 0.4f)
            {
                moveDir = (playerTransform.position - transform.position).normalized;
                moveDir.y = 0;
            }
            else moveDir = new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f,1f)).normalized;
        }

        rb.MovePosition(transform.position + moveDir * 3f * Time.deltaTime);

        // Occasionally jump
        if (jumpTimer <= 0)
        {
            jumpTimer = Random.Range(2f, 6f);
            rb.AddForce(Vector3.up * 250f);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (!alive) return;
        health -= dmg;
        if (health <= 0) Die();
    }

    void Die()
    {
        alive = false;
        if (bobluxob) bobluxob.ShowKillFeed(gameObject.name + " eliminated!");
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.material.color = Color.black;
        Destroy(gameObject, 3f);
    }
}

/// <summary>Rocket that explodes on collision.</summary>
public class BobluxobRocket : MonoBehaviour
{
    public Bobluxob bobluxob;
    public List<BobluxobNPC> npcs;

    void OnCollisionEnter(Collision col)
    {
        // Blast radius damage
        Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
        foreach (var h in hits)
        {
            BobluxobNPC npc = h.GetComponentInParent<BobluxobNPC>();
            if (npc != null) npc.TakeDamage(100);

            Rigidbody rb = h.GetComponent<Rigidbody>();
            if (rb != null) rb.AddExplosionForce(800f, transform.position, 5f);
        }

        if (bobluxob) bobluxob.ShowKillFeed("Rocket EXPLODED!");
        Destroy(gameObject);
    }
}
