using UnityEngine;
using UnityEngine.UI;

public class LoadUI : MonoBehaviour {
    [SerializeField] private Button _loadButton;

    private void Start() {
       
        _loadButton.interactable = SaveSystem.SaveExists();

        
        _loadButton.onClick.AddListener(LoadGame);
    }

    private void LoadGame() {
        if (SaveSystem.SaveExists()) {
            GameManager.Instance.LoadGame();
        } else {
            Debug.Log("No save file to load!");
        }
    }
}