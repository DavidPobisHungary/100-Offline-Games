using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Unity 5.6 compatible replacement for Vector2Int
public struct Vec2i
{
    public int x, y;
    public Vec2i(int x, int y) { this.x = x; this.y = y; }

    public static Vec2i zero  = new Vec2i(0,  0);
    public static Vec2i up    = new Vec2i(0,  1);
    public static Vec2i down  = new Vec2i(0, -1);
    public static Vec2i left  = new Vec2i(-1, 0);
    public static Vec2i right = new Vec2i(1,  0);

    public static Vec2i operator +(Vec2i a, Vec2i b) { return new Vec2i(a.x + b.x, a.y + b.y); }
    public static Vec2i operator -(Vec2i a, Vec2i b) { return new Vec2i(a.x - b.x, a.y - b.y); }
    public static Vec2i operator -(Vec2i a)           { return new Vec2i(-a.x, -a.y); }
    public static bool  operator ==(Vec2i a, Vec2i b) { return a.x == b.x && a.y == b.y; }
    public static bool  operator !=(Vec2i a, Vec2i b) { return !(a == b); }
    public override bool Equals(object obj) { if (!(obj is Vec2i)) return false; Vec2i o = (Vec2i)obj; return x == o.x && y == o.y; }
    public override int  GetHashCode() { return x * 10000 + y; }
    public override string ToString() { return "(" + x + "," + y + ")"; }
}

/// <summary>
/// PaperSnake.io - Paper.io clone with AI bots on a grid.
/// Player: blue. Bots: red, green, yellow, purple.
/// Swipe or WASD to move. Claim territory by looping back to your zone.
/// Unity 5.6.7f1 compatible (no Vector2Int).
/// </summary>
public class PaperSnake : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 30;
    public float cellSize = 20f;

    [Header("UI")]
    public RectTransform gridContainer;
    public Text scoreText;
    public Text gameOverText;
    public Button restartButton;

    private int[,] grid;
    private Image[,] cellImages;

    private Color[] playerColors = {
        new Color(0.2f, 0.5f, 1.0f),
        new Color(1.0f, 0.3f, 0.3f),
        new Color(0.3f, 0.8f, 0.3f),
        new Color(1.0f, 0.85f, 0.1f),
        new Color(0.7f, 0.3f, 1.0f),
    };
    private Color emptyColor = new Color(0.9f, 0.9f, 0.9f);

    private const int AGENT_COUNT = 5;
    private Vec2i[] headPos   = new Vec2i[AGENT_COUNT];
    private Vec2i[] direction = new Vec2i[AGENT_COUNT];
    private List<Vec2i>[] trail = new List<Vec2i>[AGENT_COUNT];
    private bool[] alive = new bool[AGENT_COUNT];

    private float moveTimer    = 0f;
    private float moveInterval = 0.12f;
    private bool  gameRunning  = false;
    private int   score        = 0;

    private Vector2 touchStart;

    void Start()
    {
        restartButton.onClick.AddListener(StartGame);
        StartGame();
    }

    void StartGame()
    {
        score = 0;
        gameRunning = true;
        gameOverText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);

        grid = new int[gridSize, gridSize];
        BuildGridUI();

        Vec2i[] startPositions = {
            new Vec2i(gridSize/2,   gridSize/2),
            new Vec2i(5,            5),
            new Vec2i(gridSize-6,   5),
            new Vec2i(5,            gridSize-6),
            new Vec2i(gridSize-6,   gridSize-6),
        };
        Vec2i[] startDirs = {
            Vec2i.right, Vec2i.right, Vec2i.left, Vec2i.up, Vec2i.down,
        };

        for (int i = 0; i < AGENT_COUNT; i++)
        {
            headPos[i]   = startPositions[i];
            direction[i] = startDirs[i];
            trail[i]     = new List<Vec2i>();
            alive[i]     = true;

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int gx = startPositions[i].x + dx;
                    int gy = startPositions[i].y + dy;
                    if (InBounds(gx, gy)) SetCell(gx, gy, i + 1);
                }
        }

        RefreshAllCells();
    }

    void BuildGridUI()
    {
        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        cellImages = new Image[gridSize, gridSize];
        float totalSize = gridSize * cellSize;
        gridContainer.sizeDelta = new Vector2(totalSize, totalSize);

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                GameObject go = new GameObject("C_" + r + "_" + c);
                go.transform.SetParent(gridContainer, false);
                RectTransform rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(cellSize - 1f, cellSize - 1f);
                rt.anchoredPosition = new Vector2(
                    c * cellSize - totalSize * 0.5f + cellSize * 0.5f,
                    r * cellSize - totalSize * 0.5f + cellSize * 0.5f
                );
                Image img = go.AddComponent<Image>();
                img.color = emptyColor;
                cellImages[r, c] = img;
            }
        }
    }

    void Update()
    {
        if (!gameRunning) return;
        HandlePlayerInput();
        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            StepBots();
            StepAllAgents();
            RefreshAllCells();
            UpdateScore();
        }
    }

    void HandlePlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))    TrySetDir(0, Vec2i.up);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))  TrySetDir(0, Vec2i.down);
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))  TrySetDir(0, Vec2i.left);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) TrySetDir(0, Vec2i.right);

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) touchStart = t.position;
            if (t.phase == TouchPhase.Ended)
            {
                Vector2 d = t.position - touchStart;
                if (d.magnitude > 30f)
                {
                    if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
                        TrySetDir(0, d.x > 0 ? Vec2i.right : Vec2i.left);
                    else
                        TrySetDir(0, d.y > 0 ? Vec2i.up : Vec2i.down);
                }
            }
        }
    }

    void TrySetDir(int agent, Vec2i newDir)
    {
        Vec2i sum = newDir + direction[agent];
        if (sum != Vec2i.zero)
            direction[agent] = newDir;
    }

    void StepBots()
    {
        for (int i = 1; i < AGENT_COUNT; i++)
        {
            if (!alive[i]) continue;
            BotAI(i);
        }
    }

    void BotAI(int bot)
    {
        if (trail[bot].Count > 15)
        {
            Vec2i toTerritory = FindOwnedNeighbour(bot);
            if (toTerritory != Vec2i.zero) { TrySetDir(bot, toTerritory); return; }
        }

        Vec2i[] options = { direction[bot], Rotate90(direction[bot]), Rotate90(-direction[bot]) };
        foreach (Vec2i d in options)
        {
            Vec2i next = headPos[bot] + d;
            if (InBounds(next.x, next.y) && !trail[bot].Contains(next))
            {
                TrySetDir(bot, d);
                return;
            }
        }
    }

    Vec2i FindOwnedNeighbour(int bot)
    {
        Vec2i[] dirs = { Vec2i.up, Vec2i.down, Vec2i.left, Vec2i.right };
        foreach (Vec2i d in dirs)
        {
            Vec2i n = headPos[bot] + d;
            if (InBounds(n.x, n.y) && grid[n.y, n.x] == (bot + 1)) return d;
        }
        return Vec2i.zero;
    }

    Vec2i Rotate90(Vec2i v) { return new Vec2i(-v.y, v.x); }

    void StepAllAgents()
    {
        for (int i = 0; i < AGENT_COUNT; i++)
        {
            if (!alive[i]) continue;

            Vec2i next = headPos[i] + direction[i];

            if (!InBounds(next.x, next.y)) { KillAgent(i); continue; }
            if (trail[i].Contains(next))   { KillAgent(i); continue; }

            bool onOwnTerritory = (grid[next.y, next.x] == (i + 1)) && trail[i].Count > 0;
            headPos[i] = next;

            if (onOwnTerritory)
            {
                ClaimTrail(i);
            }
            else
            {
                trail[i].Add(next);
                SetCell(next.x, next.y, i + 1);

                for (int e = 0; e < AGENT_COUNT; e++)
                {
                    if (e == i || !alive[e]) continue;
                    if (trail[e].Contains(next)) KillAgent(e);
                }
            }
        }
    }

    void ClaimTrail(int agent)
    {
        foreach (Vec2i cell in trail[agent])
            SetCell(cell.x, cell.y, agent + 1);
        trail[agent].Clear();
        FloodFill(agent);
    }

    void FloodFill(int agent)
    {
        bool[,] visited = new bool[gridSize, gridSize];
        Queue<Vec2i> frontier = new Queue<Vec2i>();

        for (int r = 0; r < gridSize; r++)
        {
            Enqueue(0,           r, agent, visited, frontier);
            Enqueue(gridSize-1,  r, agent, visited, frontier);
        }
        for (int c = 0; c < gridSize; c++)
        {
            Enqueue(c, 0,          agent, visited, frontier);
            Enqueue(c, gridSize-1, agent, visited, frontier);
        }

        while (frontier.Count > 0)
        {
            Vec2i p = frontier.Dequeue();
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0,  0, 1,-1 };
            for (int d = 0; d < 4; d++)
                Enqueue(p.x + dx[d], p.y + dy[d], agent, visited, frontier);
        }

        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
                if (!visited[c, r] && grid[r, c] != (agent + 1))
                    SetCell(c, r, agent + 1);
    }

    void Enqueue(int x, int y, int agent, bool[,] visited, Queue<Vec2i> q)
    {
        if (!InBounds(x, y) || visited[y, x]) return;
        if (grid[y, x] == (agent + 1)) return;
        visited[y, x] = true;
        q.Enqueue(new Vec2i(x, y));
    }

    void KillAgent(int agent)
    {
        alive[agent] = false;
        foreach (Vec2i cell in trail[agent])
            if (InBounds(cell.x, cell.y) && grid[cell.y, cell.x] == (agent + 1))
                SetCell(cell.x, cell.y, 0);
        trail[agent].Clear();

        if (agent == 0)
        {
            gameRunning = false;
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "Game Over!\nScore: " + score;
            restartButton.gameObject.SetActive(true);
        }
    }

    void SetCell(int x, int y, int owner)
    {
        if (!InBounds(x, y)) return;
        grid[y, x] = owner;
    }

    void RefreshAllCells()
    {
        if (cellImages == null) return;
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                int owner = grid[r, c];
                Color col = (owner == 0) ? emptyColor : playerColors[owner - 1];

                // Darken if trail
                for (int ag = 0; ag < AGENT_COUNT; ag++)
                {
                    if (trail[ag] != null && trail[ag].Contains(new Vec2i(c, r)))
                    {
                        col = new Color(col.r * 0.7f, col.g * 0.7f, col.b * 0.7f);
                        break;
                    }
                }

                // Brighten head
                for (int ag = 0; ag < AGENT_COUNT; ag++)
                    if (alive[ag] && headPos[ag] == new Vec2i(c, r))
                        col = Color.white;

                if (cellImages[r, c] != null) cellImages[r, c].color = col;
            }
        }
    }

    void UpdateScore()
    {
        int owned = 0;
        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
                if (grid[r, c] == 1) owned++;
        score = owned;
        if (scoreText) scoreText.text = "Territory: " + score;
        if (GameManager.Instance != null) GameManager.Instance.paperSnakeScore = score;
    }

    bool InBounds(int x, int y) { return x >= 0 && x < gridSize && y >= 0 && y < gridSize; }
}
