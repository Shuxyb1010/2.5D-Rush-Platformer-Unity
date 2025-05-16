using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public static class SaveSystem {
    private static SaveData _currentSave;
    private static bool _wasLoadedFromSave = false;

    [System.Serializable]
    public struct SaveData {
        public PlayerSaveData playerData;
        public GameSaveData gameData;
        public string currentScene;
        public List<string> collectedCoinIDs;
        public List<string> destroyedBlockIDs;
        public List<string> destroyedEnemyIDs;

        [System.NonSerialized]
        public Dictionary<string, Vector3> activeEnemyPositions;

        public List<string> savedEnemyIDs;
        public List<Vector3> savedEnemyPos;

        public void SerializeEnemyPositions() {
            if (activeEnemyPositions == null) {
                savedEnemyIDs = new List<string>();
                savedEnemyPos = new List<Vector3>();
                return;
            }
            savedEnemyIDs = new List<string>(activeEnemyPositions.Keys);
            savedEnemyPos = new List<Vector3>(activeEnemyPositions.Values);
        }

        public void DeserializeEnemyPositions() {
            activeEnemyPositions = new Dictionary<string, Vector3>();
            if (savedEnemyIDs == null || savedEnemyPos == null || savedEnemyIDs.Count != savedEnemyPos.Count) {
                savedEnemyIDs = new List<string>();
                savedEnemyPos = new List<Vector3>();
                return;
            }

            for (int i = 0; i < savedEnemyIDs.Count; i++) {
                if (!string.IsNullOrEmpty(savedEnemyIDs[i]) && !activeEnemyPositions.ContainsKey(savedEnemyIDs[i])) {
                    activeEnemyPositions.Add(savedEnemyIDs[i], savedEnemyPos[i]);
                }
            }
        }

        public void ClearSerializedLists() {
            savedEnemyIDs = null;
            savedEnemyPos = null;
        }
    }

    [System.Serializable]
    public struct PlayerSaveData {
        public Vector3 position;
        public bool isFacingRight;
        public int currentHealth;
    }

    [System.Serializable]
    public struct GameSaveData {
        public int coinsCollected;
    }

    public static void SaveGame() {
        if (GameManager.Instance == null) {
            return;
        }

        Dictionary<string, Vector3> currentActiveEnemyPositions = new Dictionary<string, Vector3>();
        EnemyStatus[] allEnemiesInScene = Object.FindObjectsOfType<EnemyStatus>(true);
        HashSet<string> destroyedIDs = GameManager.Instance.GetDestroyedEnemyIDsAsHashSet();

        foreach (EnemyStatus enemy in allEnemiesInScene) {
            if (enemy == null || string.IsNullOrEmpty(enemy.UniqueID)) continue;
            bool isDestroyed = destroyedIDs.Contains(enemy.UniqueID);
            bool isActiveHierarchy = enemy.gameObject.activeInHierarchy;
            if (!isDestroyed && isActiveHierarchy) {
                currentActiveEnemyPositions[enemy.UniqueID] = enemy.transform.position;
            }
        }

        _currentSave = new SaveData {
            playerData = new PlayerSaveData(),
            gameData = new GameSaveData { coinsCollected = GameManager.Instance.coinsCollected },
            collectedCoinIDs = GameManager.Instance.GetCollectedCoinIDs(),
            destroyedBlockIDs = GameManager.Instance.GetDestroyedBlockIDs(),
            destroyedEnemyIDs = GameManager.Instance.GetDestroyedEnemyIDs(),
            activeEnemyPositions = currentActiveEnemyPositions,
            currentScene = SceneManager.GetActiveScene().name,
        };

        PlayerMovement player = Object.FindObjectOfType<PlayerMovement>();
        if (player != null) {
            player.Save(ref _currentSave.playerData);
        }

        try {
            _currentSave.SerializeEnemyPositions();
            string json = JsonUtility.ToJson(_currentSave, true);
            File.WriteAllText(SaveFilePath(), json);
            Debug.Log("SaveSystem: Game saved successfully to: " + SaveFilePath());
        } catch (System.Exception e) {

            Debug.LogError(e);
        }
    }

    public static bool LoadGame() {
        string path = SaveFilePath();
        if (File.Exists(path)) {
            try {
                string json = File.ReadAllText(path);
                _currentSave = JsonUtility.FromJson<SaveData>(json);
                _currentSave.DeserializeEnemyPositions();
                SceneManager.LoadScene(_currentSave.currentScene);
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneLoaded += OnSceneLoaded;
                _wasLoadedFromSave = true;
                return true;
            } catch (System.Exception e) {
                _wasLoadedFromSave = false;
                return false;
            }
        } else {
            _wasLoadedFromSave = false;
            return false;
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == _currentSave.currentScene) {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (GameManager.Instance != null) {
                GameManager.Instance.StartCoroutine(DelayedApplyData());
            } else {
                _wasLoadedFromSave = false;
            }
        }
    }

    private static IEnumerator DelayedApplyData() {
        yield return new WaitForEndOfFrame();
        ApplyLoadedData();
        _wasLoadedFromSave = false;
    }

    private static void ApplyLoadedData() {
        if (GameManager.Instance == null) {
            return;
        }

        PlayerMovement player = Object.FindObjectOfType<PlayerMovement>();
        if (player != null) {
            player.Load(_currentSave.playerData);
        }

        GameManager.Instance.LoadCollectedCoins(_currentSave.collectedCoinIDs ?? new List<string>());
        GameManager.Instance.LoadDestroyedBlocks(_currentSave.destroyedBlockIDs ?? new List<string>());
        GameManager.Instance.LoadDestroyedEnemies(_currentSave.destroyedEnemyIDs ?? new List<string>());

        if (_currentSave.activeEnemyPositions != null && _currentSave.activeEnemyPositions.Count > 0) {
            foreach (KeyValuePair<string, Vector3> entry in _currentSave.activeEnemyPositions) {
                string enemyID = entry.Key;
                Vector3 savedPosition = entry.Value;
                EnemyStatus enemyInstance = GameManager.Instance.GetEnemyById(enemyID);
                if (enemyInstance != null) {
                    if (!GameManager.Instance.IsEnemyDestroyed(enemyID)) {
                        enemyInstance.transform.position = savedPosition;
                    }
                }
            }
        }

        GameManager.Instance.UpdateCoinUI();
        GameManager.Instance.UpdateHighScoreUI();
    }

    public static void DeleteSave() {
        string path = SaveFilePath();
        try {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        } catch (System.Exception e) { }

        if (GameManager.Instance != null) {
            GameManager.Instance.ResetCoins();
            GameManager.Instance.ResetBlocks();
            GameManager.Instance.ResetDestroyedEnemies();
            GameManager.Instance.ClearEnemyRegistry();
        }
    }

    public static bool SaveExists() => File.Exists(SaveFilePath());

    public static bool WasLoadedFromSave => _wasLoadedFromSave;

    private static string SaveFilePath() {
        return Path.Combine(Application.persistentDataPath, "game.save");
    }
}
