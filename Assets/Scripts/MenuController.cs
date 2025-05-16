using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour {
    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement levelSelectorInstance;


    public VisualTreeAsset levelSelectorUXML;

    private Button mainMenuStartButton;
    private Button mainMenuContinueButton;
    private Button mainMenuQuitButton;

    private void OnEnable() {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) { this.enabled = false; return; }
        root = uiDocument.rootVisualElement;
        if (root == null) { this.enabled = false; return; }

        mainMenuStartButton = root.Q<Button>("Start");
        mainMenuContinueButton = root.Q<Button>("Continue");
        mainMenuQuitButton = root.Q<Button>("Quit");

        if (mainMenuContinueButton != null) {
            mainMenuContinueButton.SetEnabled(SaveSystem.SaveExists());
        }

        if (levelSelectorInstance == null) {
            ToggleMainMenu(true);
        }

        mainMenuStartButton?.RegisterCallback<ClickEvent>(evt => ShowLevelSelector());
        mainMenuContinueButton?.RegisterCallback<ClickEvent>(evt => ContinueGame());
        mainMenuQuitButton?.RegisterCallback<ClickEvent>(evt => QuitGame());
    }

    private void OnDisable() {
        mainMenuStartButton?.UnregisterCallback<ClickEvent>(evt => ShowLevelSelector());
        mainMenuContinueButton?.UnregisterCallback<ClickEvent>(evt => ContinueGame());
        mainMenuQuitButton?.UnregisterCallback<ClickEvent>(evt => QuitGame());

        if (levelSelectorInstance != null) {
            levelSelectorInstance.Q<Button>("Back")?.UnregisterCallback<ClickEvent>(evt => CloseLevelSelector());
        }
    }

    private void ShowLevelSelector() {
        if (levelSelectorUXML == null) return;
        if (levelSelectorInstance != null) return;
        if (GameManager.Instance == null) return;

        levelSelectorInstance = levelSelectorUXML.CloneTree();
        root.Add(levelSelectorInstance);

        var level1Button = levelSelectorInstance.Q<Button>("Level1");
        var level2Button = levelSelectorInstance.Q<Button>("Level2");
        var level3Button = levelSelectorInstance.Q<Button>("Level3");
        var backButton = levelSelectorInstance.Q<Button>("Back");

        level1Button?.SetEnabled(true);

        bool level2Unlocked = GameManager.Instance.IsLevelUnlocked("Level2");
        level2Button?.SetEnabled(level2Unlocked);
        if (level2Button != null && !level2Unlocked) { level2Button.text += " (Locked)"; }

        bool level3Unlocked = GameManager.Instance.IsLevelUnlocked("Level3");
        level3Button?.SetEnabled(level3Unlocked);
        if (level3Button != null && !level3Unlocked) { level3Button.text += " (Locked)"; }

        level1Button?.RegisterCallback<ClickEvent>(evt => LoadLevel("Level1"));

        level2Button?.RegisterCallback<ClickEvent>(evt => {
            if (GameManager.Instance.IsLevelUnlocked("Level2"))
                LoadLevel("Level2");
        });

        level3Button?.RegisterCallback<ClickEvent>(evt => {
            if (GameManager.Instance.IsLevelUnlocked("Level3"))
                LoadLevel("Level3");
        });

        backButton?.RegisterCallback<ClickEvent>(evt => CloseLevelSelector());

        ToggleMainMenu(false);
    }

    private void CloseLevelSelector(bool toggleMainMenu = true) {
        if (levelSelectorInstance != null) {
            levelSelectorInstance.Q<Button>("Level1")?.UnregisterCallback<ClickEvent>(evt => LoadLevel("Level1"));
            levelSelectorInstance.Q<Button>("Level2")?.UnregisterCallback<ClickEvent>(evt => { if (GameManager.Instance.IsLevelUnlocked("Level2")) LoadLevel("Level2"); });
            levelSelectorInstance.Q<Button>("Level3")?.UnregisterCallback<ClickEvent>(evt => { if (GameManager.Instance.IsLevelUnlocked("Level3")) LoadLevel("Level3"); });
            levelSelectorInstance.Q<Button>("Back")?.UnregisterCallback<ClickEvent>(evt => CloseLevelSelector());

            root.Remove(levelSelectorInstance);
            levelSelectorInstance = null;
        }
        if (toggleMainMenu) ToggleMainMenu(true);
    }

    public void UnlockNextLevel() {
        string currentSceneName = SceneManager.GetActiveScene().name;
        string nextLevelToUnlock = null;

        if (currentSceneName == "Level1") {
            nextLevelToUnlock = "Level2";
        } else if (currentSceneName == "Level2") {
            nextLevelToUnlock = "Level3";
        }

        if (nextLevelToUnlock != null) {
            if (GameManager.Instance != null) {
                GameManager.Instance.UnlockLevel(nextLevelToUnlock);
            }
        }
    }

    private void ToggleMainMenu(bool show) {
        var targetDisplay = show ? DisplayStyle.Flex : DisplayStyle.None;

        if (mainMenuStartButton != null) {
            mainMenuStartButton.style.display = targetDisplay;
        }
        if (mainMenuQuitButton != null) {
            mainMenuQuitButton.style.display = targetDisplay;
        }

        if (mainMenuContinueButton != null) {
            mainMenuContinueButton.style.display = targetDisplay;
            if (show) {
                mainMenuContinueButton.SetEnabled(SaveSystem.SaveExists());
            }
        }
    }

    private void LoadLevel(string levelName) {
        ResetSessionStateOnly();
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelName);
    }

    private void ResetSessionStateOnly() {
        SaveSystem.DeleteSave();
    }

    public void ResetAllUnlocksAndProgress() {
        SaveSystem.DeleteSave();
        PlayerPrefs.DeleteKey("Level2_Unlocked");
        PlayerPrefs.DeleteKey("Level3_Unlocked");
        PlayerPrefs.Save();
    }

    private void ContinueGame() {
        if (SaveSystem.SaveExists()) {
            if (GameManager.Instance != null) {
                GameManager.Instance.LoadGame();
            } else {
                SceneManager.LoadScene("Level1");
                SceneManager.sceneLoaded -= LoadAfterSceneInit;
                SceneManager.sceneLoaded += LoadAfterSceneInit;
            }
        } else {
            mainMenuContinueButton?.SetEnabled(false);
        }
    }

    private void LoadAfterSceneInit(Scene scene, LoadSceneMode mode) {
        SceneManager.sceneLoaded -= LoadAfterSceneInit;
        if (GameManager.Instance != null) {
            GameManager.Instance.LoadGame();
        }
    }

    private void QuitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
