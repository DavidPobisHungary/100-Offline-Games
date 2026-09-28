using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Self-wiring Main Menu — finds all UI elements by name at runtime.
/// No Inspector assignments needed.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    private GameObject settingsPanel;

    void Start()
    {
        // Find settings panel by name
        settingsPanel = GameObject.Find("SettingsPanel");
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Wire settings icon button
        WireButton("SettingsButton", () => {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
        });

        // Wire close settings button
        WireButton("CloseBtn", () => {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        });

        // Wire game buttons
        WireButton("GameBtn_0", () => LoadGame("CookieClicker"));
        WireButton("GameBtn_1", () => LoadGame("Game2040"));
        WireButton("GameBtn_2", () => LoadGame("Bobluxob"));
        WireButton("GameBtn_3", () => LoadGame("MineBloxx"));
        WireButton("GameBtn_4", () => LoadGame("VehicleSimulator"));
        WireButton("GameBtn_5", () => LoadGame("PaperSnake"));

        // Settings sliders/toggles
        WireSlider("MusicSlider",  v => PlayerPrefs.SetFloat("MusicVol", v));
        WireSlider("SFXSlider",    v => PlayerPrefs.SetFloat("SFXVol", v));
        WireToggle("VibToggle",    v => PlayerPrefs.SetInt("Vibration", v ? 1 : 0));
    }

    void LoadGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Helper: find a Button anywhere in the scene by GO name and add listener
    void WireButton(string goName, UnityEngine.Events.UnityAction action)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning("Button not found: " + goName); return; }
        Button btn = go.GetComponent<Button>();
        if (btn == null) btn = go.GetComponentInChildren<Button>();
        if (btn == null) { Debug.LogWarning("No Button component on: " + goName); return; }
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    void WireSlider(string goName, UnityEngine.Events.UnityAction<float> action)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) return;
        Slider s = go.GetComponent<Slider>();
        if (s == null) return;
        s.onValueChanged.RemoveAllListeners();
        s.onValueChanged.AddListener(action);
    }

    void WireToggle(string goName, UnityEngine.Events.UnityAction<bool> action)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) return;
        Toggle t = go.GetComponent<Toggle>();
        if (t == null) return;
        t.onValueChanged.RemoveAllListeners();
        t.onValueChanged.AddListener(action);
    }
}
