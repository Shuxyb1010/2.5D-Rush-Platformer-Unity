using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject gameOverPanel;

    public bool isGameOver { get; private set; } = false;
    public int coinsCollected { get; private set; } = 0;

    private int currentLevelHighScore = 0;
    private string currentLevelName = "";

    private HashSet<string> _collectedCoinIDs = new HashSet<string>();
    private HashSet<string> _destroyedBlockIDs = new HashSet<string>();
    private HashSet<string> _destroyedEnemyIDs = new HashSet<string>();
    private Dictionary<string, EnemyStatus> _activeEnemyRegistry = new Dictionary<string, EnemyStatus>();

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            isGameOver = false;
            Time.timeScale = 1f;
            ClearEnemyRegistry();
        } else {
            Destroy(gameObject);
        }
    }

    void OnDestroy() {
        if (Instance == this) {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        currentLevelName = scene.name;
        string highScoreKey = "HighScore:" + currentLevelName;
        currentLevelHighScore = PlayerPrefs.GetInt(highScoreKey, 0);

        FindCoinText();
        FindHighScoreText();
        FindAndInitializeGameOverPanel();
        
        isGameOver = false;
        Time.timeScale = 1f;


        UpdateCoinUI();
        UpdateHighScoreUI();
    }

    private void FindAndInitializeGameOverPanel() {
        var panelObj = GameObject.FindGameObjectWithTag("GameOverPanel");
        if (panelObj != null) {
            gameOverPanel = panelObj;
            gameOverPanel.SetActive(false);
        } else {
            gameOverPanel = null;
        }
    }

    private void FindHighScoreText() {
        var textObj = GameObject.FindGameObjectWithTag("HighScoreText");
        if (textObj != null) {
            highScoreText = textObj.GetComponent<TextMeshProUGUI>();
        }
    }

    public void UpdateHighScoreUI() {
        if (highScoreText != null) {
            highScoreText.text = "High Score: " + currentLevelHighScore.ToString();
        }
    }

    private void FindCoinText() {
        var textObj = GameObject.FindGameObjectWithTag("CoinText");
        if (textObj != null) {
            coinText = textObj.GetComponent<TextMeshProUGUI>();
        }
    }

    public void UpdateCoinUI() {
        if (coinText != null) {
            coinText.text = "Score: " + coinsCollected.ToString();
        }
        
    }

    public void AddScore(int pointsToAdd) {
        if (pointsToAdd <= 0 || isGameOver) return;

        coinsCollected += pointsToAdd;
        UpdateCoinUI();

        if (coinsCollected > currentLevelHighScore) {
            currentLevelHighScore = coinsCollected;
            UpdateHighScoreUI();
            CheckAndSaveLevelHighScore();
        }
    }

    public void AddCoin(Coin coin) {
        if (coin == null) return;
        string coinID = coin.GetCoinID();
        if (_collectedCoinIDs.Add(coinID)) {
            coinsCollected++; 
            UpdateCoinUI();

            if (coinsCollected > currentLevelHighScore) {
                currentLevelHighScore = coinsCollected;
                UpdateHighScoreUI();
                
            }
        }
    }

    public void TriggerGameOver() {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;

        CheckAndSaveLevelHighScore(); 

        if (gameOverPanel == null) { FindAndInitializeGameOverPanel(); }
        if (gameOverPanel != null) { gameOverPanel.SetActive(true); }
    }

    
    public void CheckAndSaveLevelHighScore() {
        if (string.IsNullOrEmpty(currentLevelName)) return;

        string highScoreKey = "HighScore_" + currentLevelName;
        int savedHighScore = PlayerPrefs.GetInt(highScoreKey, 0);

        if (coinsCollected > savedHighScore) {
            currentLevelHighScore = coinsCollected; 
            PlayerPrefs.SetInt(highScoreKey, currentLevelHighScore);
            PlayerPrefs.Save();
            
        }
    }

    public List<string> GetCollectedCoinIDs() {
        return new List<string>(_collectedCoinIDs);
    }

    public void LoadCollectedCoins(List<string> coinIDs) {
        _collectedCoinIDs = (coinIDs != null) ? new HashSet<string>(coinIDs) : new HashSet<string>();
    }

    public void LoadScore(int loadedScore) {
        coinsCollected = loadedScore;
        UpdateCoinUI();
        
        if (coinsCollected > currentLevelHighScore) {
            currentLevelHighScore = coinsCollected;
            UpdateHighScoreUI();
        }
    }


    public bool IsCoinCollected(string coinID) {
        return _collectedCoinIDs.Contains(coinID);
    }

    public void SaveGame() => SaveSystem.SaveGame();

    public void LoadGame() {
        if (!SaveSystem.LoadGame()) {
            
        }
    }

    public void UnlockLevel(string levelName) {
        if (!string.IsNullOrEmpty(levelName)) {
            PlayerPrefs.SetInt(levelName + "_Unlocked", 1);
            PlayerPrefs.Save();
        }
    }

    public bool IsLevelUnlocked(string levelName) {
        return PlayerPrefs.GetInt(levelName + "_Unlocked", 0) == 1;
    }

    public void ResetCoins() {
        coinsCollected = 0;
        _collectedCoinIDs.Clear();
        // UpdateCoinUI(); 
    }

    public void AddDestroyedBlock(string blockID) {
        if (!string.IsNullOrEmpty(blockID) && _destroyedBlockIDs.Add(blockID)) {
            SaveGame();
        }
    }

    public bool IsBlockDestroyed(string blockID) {
        return !string.IsNullOrEmpty(blockID) && _destroyedBlockIDs.Contains(blockID);
    }

    public void LoadDestroyedBlocks(List<string> blockIDs) {
        _destroyedBlockIDs = (blockIDs != null) ? new HashSet<string>(blockIDs) : new HashSet<string>();
    }

    public List<string> GetDestroyedBlockIDs() {
        return new List<string>(_destroyedBlockIDs);
    }

    public void ResetBlocks() {
        _destroyedBlockIDs.Clear();
    }

    public void AddDestroyedEnemy(string enemyID) {
        if (string.IsNullOrEmpty(enemyID)) return;
        if (_destroyedEnemyIDs.Add(enemyID)) {
            if (_activeEnemyRegistry.TryGetValue(enemyID, out EnemyStatus enemyToUnregister)) {
                UnregisterEnemy(enemyToUnregister);
            }
        }
    }

    public void LoadDestroyedEnemies(List<string> enemyIDs) {
        _destroyedEnemyIDs = (enemyIDs != null) ? new HashSet<string>(enemyIDs) : new HashSet<string>();
    }

    public void ResetDestroyedEnemies() {
        _destroyedEnemyIDs.Clear();
    }

    public bool IsEnemyDestroyed(string enemyID) {
        return !string.IsNullOrEmpty(enemyID) && _destroyedEnemyIDs.Contains(enemyID);
    }

    public List<string> GetDestroyedEnemyIDs() {
        return new List<string>(_destroyedEnemyIDs);
    }

    public HashSet<string> GetDestroyedEnemyIDsAsHashSet() {
        return _destroyedEnemyIDs;
    }

    public void RegisterEnemy(EnemyStatus enemy) {
        if (enemy == null || string.IsNullOrEmpty(enemy.UniqueID)) return;
        _activeEnemyRegistry[enemy.UniqueID] = enemy;
    }

    public void UnregisterEnemy(EnemyStatus enemy) {
        if (enemy == null || string.IsNullOrEmpty(enemy.UniqueID)) return;
        _activeEnemyRegistry.Remove(enemy.UniqueID);
    }

    public EnemyStatus GetEnemyById(string uniqueID) {
        if (string.IsNullOrEmpty(uniqueID)) return null;
        _activeEnemyRegistry.TryGetValue(uniqueID, out EnemyStatus enemy);
        return enemy;
    }

    public void ClearEnemyRegistry() {
        _activeEnemyRegistry.Clear();
    }

    public void RestartGame() {
        Time.timeScale = 1f;
        isGameOver = false;

        if (InputManager.Instance != null) {
            
            InputManager.Instance.StopAllCoroutines();
            InputManager.Instance.StartCoroutine(
                InputManager.Instance.HandleSceneTransitionInput()
            );
        }

        CheckAndSaveLevelHighScore(); 
        ResetCoins();
        ResetBlocks();
        ResetDestroyedEnemies();
        ClearEnemyRegistry();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu(string menuSceneName = "MainMenu") {
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame() {
        CheckAndSaveLevelHighScore(); 
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}