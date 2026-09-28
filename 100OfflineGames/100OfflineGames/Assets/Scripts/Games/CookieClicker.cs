using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full Cookie Clicker implementation.
/// Scene needs: CookieButton, CookieCountText, UpgradesPanel,
/// UpgradesButton, and one UpgradeItem prefab per upgrade row.
/// </summary>
public class CookieClicker : MonoBehaviour
{
    [Header("UI References")]
    public Text cookieCountText;
    public Button cookieButton;
    public Button upgradesToggleButton;
    public GameObject upgradesPanel;
    public Text upgradePanelTitle;

    [Header("Upgrade Buttons (in order)")]
    public Button[] upgradeButtons;   // 7 buttons
    public Text[]   upgradeCostTexts; // matching cost labels
    public Text[]   upgradeNameTexts; // matching name labels

    [Header("Cookie Image")]
    public RectTransform cookieRect;

    // ---- Upgrade data ----
    private string[] upgradeNames = {
        "+1 Click/Click",
        "+20 Click/Click",
        "+100 Click/Click",
        "+1 AutoClick/sec",
        "+80 AutoClick/sec",
        "Ultra Fun Mode!",
        "Finish & End Game"
    };

    private long[] baseCosts = { 10, 30, 90, 300, 800, 10000, 1000000 };
    private long[] currentCosts;
    private int[]  purchaseCounts;

    // Upgrades 0-2: clicks per click bonus
    // Upgrades 3-4: auto click bonus
    // Upgrade  5  : ultra fun (cosmetic)
    // Upgrade  6  : end game

    private long  cookies;
    private int   clicksPerClick = 1;
    private long  autoClickPerSec = 0;
    private bool  ultraFunMode = false;

    private float autoClickTimer = 0f;
    private float cookieBounce = 0f;
    private bool  gameEnded = false;

    void Start()
    {
        // Load from GameManager if present
        if (GameManager.Instance != null)
        {
            cookies        = GameManager.Instance.cookieCount;
            clicksPerClick = GameManager.Instance.clicksPerClick;
            autoClickPerSec= GameManager.Instance.autoClickPerSecond;
            ultraFunMode   = GameManager.Instance.ultraFunMode;
        }

        currentCosts   = (long[])baseCosts.Clone();
        purchaseCounts = new int[7];

        upgradesPanel.SetActive(false);

        cookieButton.onClick.AddListener(OnCookieClick);
        upgradesToggleButton.onClick.AddListener(() => upgradesPanel.SetActive(!upgradesPanel.activeSelf));

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int idx = i;
            upgradeButtons[i].onClick.AddListener(() => TryBuyUpgrade(idx));
        }

        RefreshUI();
    }

    void Update()
    {
        if (gameEnded) return;

        // AutoClick
        if (autoClickPerSec > 0)
        {
            autoClickTimer += Time.deltaTime;
            if (autoClickTimer >= 1f)
            {
                autoClickTimer -= 1f;
                AddCookies(autoClickPerSec);
            }
        }

        // Cookie bounce animation
        if (cookieBounce > 0f)
        {
            cookieBounce -= Time.deltaTime * 8f;
            float s = 1f + Mathf.Sin(cookieBounce * Mathf.PI) * 0.08f;
            if (cookieRect) cookieRect.localScale = Vector3.one * s;
        }

        // Ultra Fun Mode rainbow spin
        if (ultraFunMode && cookieRect)
        {
            cookieRect.Rotate(0, 0, 90f * Time.deltaTime);
        }
    }

    void OnCookieClick()
    {
        if (gameEnded) return;
        AddCookies(clicksPerClick);
        cookieBounce = 1f;
    }

    void AddCookies(long amount)
    {
        cookies += amount;
        if (GameManager.Instance != null) GameManager.Instance.cookieCount = cookies;
        RefreshCookieText();
        RefreshUpgradeCosts();
    }

    void TryBuyUpgrade(int idx)
    {
        if (cookies < currentCosts[idx]) return;

        cookies -= currentCosts[idx];
        purchaseCounts[idx]++;

        // Price goes up each purchase
        currentCosts[idx] = (long)(baseCosts[idx] * Mathf.Pow(1.5f, purchaseCounts[idx]));

        ApplyUpgrade(idx);
        RefreshUI();
    }

    void ApplyUpgrade(int idx)
    {
        switch (idx)
        {
            case 0: clicksPerClick += 1;   break;
            case 1: clicksPerClick += 20;  break;
            case 2: clicksPerClick += 100; break;
            case 3: autoClickPerSec += 1;  break;
            case 4: autoClickPerSec += 80; break;
            case 5:
                ultraFunMode = true;
                if (GameManager.Instance != null) GameManager.Instance.ultraFunMode = true;
                break;
            case 6:
                EndGame();
                break;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.clicksPerClick    = clicksPerClick;
            GameManager.Instance.autoClickPerSecond = autoClickPerSec;
        }
    }

    void EndGame()
    {
        gameEnded = true;
        upgradesPanel.SetActive(false);
        cookieButton.interactable = false;

        // Show end message by repurposing upgrade panel
        upgradesPanel.SetActive(true);
        if (upgradePanelTitle) upgradePanelTitle.text =
            "YOU WIN!\nYou spent 1,000,000 cookies!\nYou finished Cookie Clicker!\n\nPress the Exit button to go back to the menu.";
    }

    void RefreshUI()
    {
        RefreshCookieText();
        RefreshUpgradeCosts();
    }

    void RefreshCookieText()
    {
        if (cookieCountText)
            cookieCountText.text = "Cookies: " + FormatNumber(cookies);
    }

    void RefreshUpgradeCosts()
    {
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (upgradeNameTexts != null && i < upgradeNameTexts.Length && upgradeNameTexts[i])
                upgradeNameTexts[i].text = upgradeNames[i];

            if (upgradeCostTexts != null && i < upgradeCostTexts.Length && upgradeCostTexts[i])
                upgradeCostTexts[i].text = "Cost: " + FormatNumber(currentCosts[i]);

            if (upgradeButtons[i])
                upgradeButtons[i].interactable = (cookies >= currentCosts[i]) && !gameEnded;
        }
    }

    string FormatNumber(long n)
    {
        if (n >= 1000000000L) return (n / 1000000000f).ToString("0.#") + "B";
        if (n >= 1000000L)    return (n / 1000000f).ToString("0.#") + "M";
        if (n >= 1000L)       return (n / 1000f).ToString("0.#") + "K";
        return n.ToString();
    }
}
