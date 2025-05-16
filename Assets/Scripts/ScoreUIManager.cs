using TMPro;
using UnityEngine;

public class ScoreUIManager : MonoBehaviour {
    public static ScoreUIManager Instance { get; private set; }

    [Header("Coin UI")]
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void UpdateCoinUI(int coins) {
        if (coinText != null)
            coinText.text = $"Coins: {coins}";
    }

    public void UpdateScoreDisplay(int score) {
        if (currentScoreText != null)
            currentScoreText.text = $"Score: {score}";
    }

    public void UpdateHighScoreDisplay(int highScore) {
        if (highScoreText != null)
            highScoreText.text = $"High Score: {highScore}";
    }
}