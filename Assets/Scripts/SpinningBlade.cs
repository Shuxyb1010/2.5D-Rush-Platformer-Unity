using UnityEngine;
using System.Collections; 

public class SpinningBladeDamage : MonoBehaviour {

    public int damageAmount = 15;

    public float damageCooldown = 0.5f;


    private bool canDamage = true; 



    
    private void OnTriggerStay(Collider other)
    {
        
        if (!canDamage) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {

                playerHealth.TakeDamage(damageAmount);
                StartCoroutine(DamageCooldownRoutine());
            }
        }
    }



    private IEnumerator DamageCooldownRoutine() {
        canDamage = false; 
        yield return new WaitForSeconds(damageCooldown);
        canDamage = true; 
    }

    private void OnEnable() {
        canDamage = true;
        StopAllCoroutines(); 
    }
}