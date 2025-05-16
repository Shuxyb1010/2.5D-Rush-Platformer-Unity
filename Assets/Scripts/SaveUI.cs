using UnityEngine;
using UnityEngine.UI;

public class SaveUI : MonoBehaviour {
    [SerializeField] private Button _saveButton;

    private void Start() {
        _saveButton.onClick.AddListener(SaveGame);
    }

    private void SaveGame() {
        GameManager.Instance.SaveGame();
        GameManager.Instance.CheckAndSaveLevelHighScore();

    }
}