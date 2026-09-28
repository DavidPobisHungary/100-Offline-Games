using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Self-wiring in-game HUD — finds buttons by name, no Inspector needed.
/// Portrait  -> bar at TOP
/// Landscape -> bar at RIGHT
/// </summary>
public class InGameHUD : MonoBehaviour
{
    private GameObject settingsPopup;
    private RectTransform hudPanel;
    private ScreenOrientation lastOrientation;

    void Start()
    {
        hudPanel = GetComponent<RectTransform>();

        settingsPopup = GameObject.Find("InGameSettingsPopup");
        if (settingsPopup != null) settingsPopup.SetActive(false);

        WireButton("SettingsBtn", () => {
            if (settingsPopup != null)
                settingsPopup.SetActive(!settingsPopup.activeSelf);
        });

        WireButton("SaveBtn", () => {
            if (GameManager.Instance != null) GameManager.Instance.SaveProgress();
            Debug.Log("Progress Saved!");
        });

        WireButton("ExitBtn", () => {
            if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
            else SceneManager.LoadScene("MainMenu");
        });

        WireButton("ClosePopup", () => {
            if (settingsPopup != null) settingsPopup.SetActive(false);
        });

        lastOrientation = Screen.orientation;
        LayoutHUD();
    }

    void Update()
    {
        if (Screen.orientation != lastOrientation)
        {
            lastOrientation = Screen.orientation;
            LayoutHUD();
        }
    }

    void LayoutHUD()
    {
        if (hudPanel == null) return;
        bool portrait = Screen.height >= Screen.width;
        if (portrait)
        {
            hudPanel.anchorMin        = new Vector2(0f, 1f);
            hudPanel.anchorMax        = new Vector2(1f, 1f);
            hudPanel.pivot            = new Vector2(0.5f, 1f);
            hudPanel.sizeDelta        = new Vector2(0f, 80f);
            hudPanel.anchoredPosition = Vector2.zero;
        }
        else
        {
            hudPanel.anchorMin        = new Vector2(1f, 0f);
            hudPanel.anchorMax        = new Vector2(1f, 1f);
            hudPanel.pivot            = new Vector2(1f, 0.5f);
            hudPanel.sizeDelta        = new Vector2(80f, 0f);
            hudPanel.anchoredPosition = Vector2.zero;
        }
    }

    void WireButton(string goName, UnityEngine.Events.UnityAction action)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) return;
        Button btn = go.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }
}
