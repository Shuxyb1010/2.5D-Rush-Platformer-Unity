using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBarUI : MonoBehaviour {
    private Slider healthSlider;
    private PlayerHealth playerHealth;

    void Awake() {
        healthSlider = GetComponent<Slider>();
        if (healthSlider == null) {
            this.enabled = false;
        }
    }

    void Start() {
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth != null) {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
            playerHealth.OnHealthChanged += UpdateHealthBar;

            if (playerHealth.MaxHealth > 0) {
                UpdateHealthBar((float)playerHealth.CurrentHealth / playerHealth.MaxHealth);
            } else {
                UpdateHealthBar(0f);
            }
        } else {
            if (healthSlider != null) healthSlider.gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBar(float currentHealthRatio) {
        if (healthSlider != null) {
            healthSlider.value = Mathf.Clamp01(currentHealthRatio);
        }
    }

    void OnDestroy() {
        if (playerHealth != null) {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }
}
