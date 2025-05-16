using UnityEngine;
using System;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour {
    [Header("Health Settings")]
    [SerializeField]
    private int maxHealth = 100;

    public int CurrentHealth { get; private set; }

    public int MaxHealth => maxHealth;

    public event Action<float> OnHealthChanged;

    public UnityEvent OnPlayerDeath;

    void Awake() {
        CurrentHealth = maxHealth;
    }

    void Start() {
        UpdateHealthBar();
    }

    public int GetCurrentHealth() {
        return CurrentHealth;
    }

    public void SetHealth(int loadedHealth) {
        CurrentHealth = Mathf.Clamp(loadedHealth, 0, maxHealth);

        UpdateHealthBar();

        if (CurrentHealth <= 0) {
            Die();
        }
    }

    public void TakeDamage(int damageAmount) {
        if (damageAmount <= 0 || CurrentHealth <= 0) return;

        CurrentHealth -= damageAmount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

        UpdateHealthBar();

        if (CurrentHealth <= 0) {
            Die();
        }
    }

    public void Heal(int healAmount) {
        if (healAmount <= 0 || CurrentHealth <= 0 || CurrentHealth >= maxHealth) return;

        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

        UpdateHealthBar();
    }

    private void UpdateHealthBar() {
        float currentHealthRatio = (float)CurrentHealth / MaxHealth;
        OnHealthChanged?.Invoke(currentHealthRatio);
    }

    private void Die() {
        OnPlayerDeath?.Invoke();

        if (GameManager.Instance != null) {
            GameManager.Instance.TriggerGameOver();
        }
    }
}
