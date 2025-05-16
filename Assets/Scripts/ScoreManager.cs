using UnityEngine;

public class ScoreManager : MonoBehaviour {
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }

    private const string HighScoreKey = "EncryptedHighScore";
    private const int EncryptionKey = 0x2A3B4C5D;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScore();
        } else {
            Destroy(gameObject);
        }
    }

    public void AddPoints(int points) {
        CurrentScore += points;
        if (CurrentScore > HighScore) {
            HighScore = CurrentScore;
            SaveHighScore();
        }
    }

    public void ResetCurrentScore() => CurrentScore = 0;

    public void SaveHighScore() =>
        PlayerPrefs.SetInt(HighScoreKey, HighScore ^ EncryptionKey);

    private void LoadHighScore() =>
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0) ^ EncryptionKey;
}