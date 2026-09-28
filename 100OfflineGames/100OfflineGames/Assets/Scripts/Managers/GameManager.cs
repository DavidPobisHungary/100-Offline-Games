using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Cookie Clicker save
    public long cookieCount = 0;
    public int clicksPerClick = 1;
    public long autoClickPerSecond = 0;
    public bool ultraFunMode = false;

    // 2040 save
    public int[] board2040 = new int[16];
    public int score2040 = 0;

    // PaperSnake save
    public int paperSnakeScore = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetString("CookieCount", cookieCount.ToString());
        PlayerPrefs.SetInt("ClicksPerClick", clicksPerClick);
        PlayerPrefs.SetString("AutoClickPerSecond", autoClickPerSecond.ToString());
        PlayerPrefs.SetInt("UltraFunMode", ultraFunMode ? 1 : 0);
        PlayerPrefs.SetInt("Score2040", score2040);
        PlayerPrefs.SetInt("PaperSnakeScore", paperSnakeScore);
        for (int i = 0; i < 16; i++)
            PlayerPrefs.SetInt("Board2040_" + i, board2040[i]);
        PlayerPrefs.Save();
        Debug.Log("Progress Saved!");
    }

    public void LoadProgress()
    {
        cookieCount = long.Parse(PlayerPrefs.GetString("CookieCount", "0"));
        clicksPerClick = PlayerPrefs.GetInt("ClicksPerClick", 1);
        autoClickPerSecond = long.Parse(PlayerPrefs.GetString("AutoClickPerSecond", "0"));
        ultraFunMode = PlayerPrefs.GetInt("UltraFunMode", 0) == 1;
        score2040 = PlayerPrefs.GetInt("Score2040", 0);
        paperSnakeScore = PlayerPrefs.GetInt("PaperSnakeScore", 0);
        for (int i = 0; i < 16; i++)
            board2040[i] = PlayerPrefs.GetInt("Board2040_" + i, 0);
        Debug.Log("Progress Loaded!");
    }

    public void LaunchGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ReturnToMainMenu()
    {
        SaveProgress();
        SceneManager.LoadScene("MainMenu");
    }
}
