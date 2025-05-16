using System.Collections;
using UnityEngine;

public class ProjectileBase : MonoBehaviour {
    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDuration = 2f;
    [SerializeField] private float projectileGravityMultiplier = 0f;
    [SerializeField] private string targetTag = "Enemy";
    [SerializeField] private float shootShakeForce = 0.1f;


    private float timeElapsed;
    private Rigidbody projectileRb;

    private void Awake() {
        projectileRb = GetComponent<Rigidbody>();
        if (projectileRb) {
            projectileRb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
            projectileRb.useGravity = projectileGravityMultiplier > 0;
        }
      
    }



    private void Update() {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > projectileDuration) 
            ProjectileDecay();
    }



    public void Launch(Vector2 direction) {
        
        if (direction.x > 0) {
            transform.rotation = Quaternion.identity; 
        } else if (direction.x < 0) {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        Vector3 launchDirection = new Vector3(direction.x, 0f, 0f);
        projectileRb.linearVelocity = launchDirection * projectileSpeed;
    }



    private void ProjectileDecay() {
        timeElapsed = 0.0f;

       
        transform.rotation = Quaternion.identity;

        Pool.instance.Return(gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(targetTag)) {
            CameraShake.Instance.Shake(shootShakeForce);

            ProjectileDecay();

        }
    }
}