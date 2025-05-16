using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelExit : MonoBehaviour {
    
    [SerializeField] private string nextLevelName = "Level2";
    [SerializeField] private bool isFinalLevel = false;
    [SerializeField] private float loadDelay = 1.0f;

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other) {
        if (isLoading) return;

        if (other.CompareTag("Player")) {
            isLoading = true;

            if (!isFinalLevel) {
                if (string.IsNullOrEmpty(nextLevelName)) {
                    isLoading = false;
                    return;
                }

                if (GameManager.Instance != null) {
                    GameManager.Instance.UnlockLevel(nextLevelName);
                }
            }

            if (GameManager.Instance != null) {
                GameManager.Instance.SaveGame();
            }

            if (other.TryGetComponent<PlayerMovement>(out var playerMovement)) {
                playerMovement.enabled = false;
            }

            StartCoroutine(LoadNextLevelSequence());
        }
    }

    private IEnumerator LoadNextLevelSequence() {
        yield return new WaitForSeconds(loadDelay);

        Time.timeScale = 1f;

        if (isFinalLevel) {
            SceneManager.LoadScene("MainMenu");
        } else if (!string.IsNullOrEmpty(nextLevelName)) {
            SceneManager.LoadScene(nextLevelName);
        } else {
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void OnDrawGizmos() {
        Collider col = GetComponent<Collider>();
        if (col != null) {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position + col.bounds.center - transform.position, col.bounds.size);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position + col.bounds.center - transform.position, col.bounds.size);
        }
    }
}
