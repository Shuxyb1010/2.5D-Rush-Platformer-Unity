using UnityEngine;
using TMPro;

public class CoinCollection : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    [SerializeField] private AudioClip collectSound; 

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {

          
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            
            Destroy(gameObject);
        }
    }

}
