using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 2040 - a 2048 clone.
/// 4x4 grid. Swipe or arrow keys to merge tiles.
/// Scene needs a 4x4 grid of Text UI elements assigned to tileTexts[0..15].
/// </summary>
public class Game2040 : MonoBehaviour
{
    [Header("UI")]
    public Text[] tileTexts;   // 16 texts, row-major (0=top-left)
    public Text   scoreText;
    public Text   bestText;
    public Button newGameButton;

    private int[,] board = new int[4, 4];
    private int score = 0;
    private int best  = 0;
    private bool gameOver = false;

    // Swipe detection
    private Vector2 touchStart;
    private bool    touching = false;
    private const float SWIPE_THRESHOLD = 50f;

    // Tile colours
    private static readonly Color[] tileColors = {
        new Color(0.80f, 0.75f, 0.71f), // 0    empty
        new Color(0.93f, 0.89f, 0.85f), // 2
        new Color(0.93f, 0.87f, 0.78f), // 4
        new Color(0.95f, 0.69f, 0.47f), // 8
        new Color(0.96f, 0.58f, 0.39f), // 16
        new Color(0.96f, 0.49f, 0.37f), // 32
        new Color(0.96f, 0.37f, 0.23f), // 64
        new Color(0.93f, 0.81f, 0.45f), // 128
        new Color(0.93f, 0.80f, 0.38f), // 256
        new Color(0.93f, 0.78f, 0.31f), // 512
        new Color(0.93f, 0.77f, 0.25f), // 1024
        new Color(0.93f, 0.76f, 0.18f), // 2040
    };

    void Start()
    {
        best = PlayerPrefs.GetInt("2040Best", 0);
        newGameButton.onClick.AddListener(NewGame);
        NewGame();
    }

    void NewGame()
    {
        score = 0;
        gameOver = false;
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                board[r, c] = 0;

        SpawnTile();
        SpawnTile();
        RefreshUI();
    }

    void Update()
    {
        if (gameOver) return;
        HandleInput();
    }

    void HandleInput()
    {
        // Keyboard
        if (Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W)) Move(0);
        if (Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S)) Move(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A)) Move(2);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Move(3);

        // Touch swipe
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)  { touchStart = t.position; touching = true; }
            if (t.phase == TouchPhase.Ended && touching)
            {
                touching = false;
                Vector2 delta = t.position - touchStart;
                if (delta.magnitude > SWIPE_THRESHOLD)
                {
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        Move(delta.x > 0 ? 3 : 2);
                    else
                        Move(delta.y > 0 ? 0 : 1);
                }
            }
        }
    }

    // dir: 0=up 1=down 2=left 3=right
    void Move(int dir)
    {
        bool moved = false;

        if (dir == 2 || dir == 3) // horizontal
        {
            for (int r = 0; r < 4; r++)
            {
                int[] row = GetRow(r);
                int[] merged = Slide(row, dir == 3);
                if (!ArrayEqual(row, merged)) moved = true;
                SetRow(r, merged);
            }
        }
        else // vertical
        {
            for (int c = 0; c < 4; c++)
            {
                int[] col = GetCol(c);
                int[] merged = Slide(col, dir == 1);
                if (!ArrayEqual(col, merged)) moved = true;
                SetCol(c, merged);
            }
        }

        if (moved)
        {
            SpawnTile();
            CheckGameOver();
            RefreshUI();
        }
    }

    int[] Slide(int[] line, bool reverse)
    {
        if (reverse) System.Array.Reverse(line);

        // Compact
        List<int> tiles = new List<int>();
        foreach (int v in line) if (v != 0) tiles.Add(v);

        // Merge
        for (int i = 0; i < tiles.Count - 1; i++)
        {
            if (tiles[i] == tiles[i + 1])
            {
                tiles[i] *= 2;
                score += tiles[i];
                if (score > best) { best = score; PlayerPrefs.SetInt("2040Best", best); }
                tiles.RemoveAt(i + 1);
            }
        }

        // Pad
        while (tiles.Count < 4) tiles.Add(0);
        int[] result = tiles.ToArray();
        if (reverse) System.Array.Reverse(result);
        return result;
    }

    void SpawnTile()
    {
        List<int> emptyR = new List<int>();
        List<int> emptyC = new List<int>();
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                if (board[r, c] == 0) { emptyR.Add(r); emptyC.Add(c); }

        if (emptyR.Count == 0) return;
        int idx = Random.Range(0, emptyR.Count);
        board[emptyR[idx], emptyC[idx]] = Random.value < 0.9f ? 2 : 4;
    }

    void CheckGameOver()
    {
        // Any empty? fine
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                if (board[r, c] == 0) return;

        // Any adjacent match?
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                if (r < 3 && board[r, c] == board[r + 1, c]) return;
                if (c < 3 && board[r, c] == board[r, c + 1]) return;
            }

        gameOver = true;
    }

    void RefreshUI()
    {
        if (scoreText) scoreText.text = "Score: " + score;
        if (bestText)  bestText.text  = "Best: "  + best;

        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                int idx = r * 4 + c;
                int val = board[r, c];
                if (idx >= tileTexts.Length || tileTexts[idx] == null) continue;

                tileTexts[idx].text = val == 0 ? "" : val.ToString();

                // Background colour via parent image if present
                Image bg = tileTexts[idx].transform.parent
                           ? tileTexts[idx].transform.parent.GetComponent<Image>()
                           : null;
                if (bg != null)
                {
                    int colorIdx = val == 0 ? 0 : Mathf.Clamp((int)Mathf.Log(val, 2), 1, tileColors.Length - 1);
                    bg.color = tileColors[colorIdx];
                }

                // Text colour
                tileTexts[idx].color = val <= 4 ? new Color(0.47f, 0.43f, 0.40f) : Color.white;
                tileTexts[idx].fontSize = val >= 1000 ? 28 : val >= 100 ? 34 : 42;
            }
        }
    }

    // --- Helpers ---
    int[] GetRow(int r) { return new int[]{ board[r,0], board[r,1], board[r,2], board[r,3] }; }
    void  SetRow(int r, int[] v) { board[r,0]=v[0]; board[r,1]=v[1]; board[r,2]=v[2]; board[r,3]=v[3]; }
    int[] GetCol(int c) { return new int[]{ board[0,c], board[1,c], board[2,c], board[3,c] }; }
    void  SetCol(int c, int[] v) { board[0,c]=v[0]; board[1,c]=v[1]; board[2,c]=v[2]; board[3,c]=v[3]; }

    bool ArrayEqual(int[] a, int[] b)
    {
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }
}
