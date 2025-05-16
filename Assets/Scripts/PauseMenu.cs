using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    [SerializeField] GameObject pauseMenu;

    public void Pause() {

        pauseMenu.SetActive(true);
        Time.timeScale = 0;

    }

    public void Home() {

        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1;
    }

    public void Resume() {

        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void Restart() {
        Time.timeScale = 1;

        if (InputManager.Instance != null) {
            InputManager.Instance.StopAllCoroutines();
            InputManager.Instance.StartCoroutine(
                InputManager.Instance.HandleSceneTransitionInput()
            );
        }
        
        GameManager.Instance.ResetCoins();
        GameManager.Instance.ResetBlocks();
        GameManager.Instance.ResetDestroyedEnemies();
        GameManager.Instance.ClearEnemyRegistry();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
