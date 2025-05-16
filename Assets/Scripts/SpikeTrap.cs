using UnityEngine;

public class SpikeDamageOnState : MonoBehaviour {

    public int damageAmount = 20; 

    public Animator trapAnimator;

    public string dangerousAnimationStateName = "Spikes_Extended";

    void Awake() {
        
        if (trapAnimator == null) {
          
            this.enabled = false;
        }
        if (string.IsNullOrEmpty(dangerousAnimationStateName)) {
            
            this.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other) {
        
        if (trapAnimator == null) return;

       
        if (other.CompareTag("Player")) {

            AnimatorStateInfo currentStateInfo = trapAnimator.GetCurrentAnimatorStateInfo(0);

            if (currentStateInfo.IsName(dangerousAnimationStateName)) {
               
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null) {
                   
                    playerHealth.TakeDamage(damageAmount);
                } else {
                    Debug.LogWarning("PlayerHealth not found!", other);
                }
            }
        }
    }
}