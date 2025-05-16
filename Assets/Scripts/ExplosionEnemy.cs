using UnityEngine;

[RequireComponent(typeof(EnemyStatus))]
public class ExplosionEnemy : MonoBehaviour {
    [SerializeField] private int maxNumOfHits;
    [SerializeField] private FireProjectile fireProjectile;
    [SerializeField] private float hitOffset = -1.5f;
    [SerializeField] private float destroyShakeForce = 0.3f;

    private int numOfHits;
    private ProjectileBase projectileBase;
    private EnemyStatus enemyStatus;

    [SerializeField] private int scoreValue = 10;

    private void Awake() {
        enemyStatus = GetComponent<EnemyStatus>();
        if (enemyStatus == null) {
            enabled = false;
            return;
        }
    }

    private void Start() {
        numOfHits = maxNumOfHits;

        if (GameManager.Instance != null && enemyStatus != null && !string.IsNullOrEmpty(enemyStatus.UniqueID)) {
            if (GameManager.Instance.IsEnemyDestroyed(enemyStatus.UniqueID)) {
                Destroy(gameObject);
                return;
            }
        }
    }

    private void OnTriggerEnter(Collider collision) {
        if (collision.CompareTag("Projectile")) {
            projectileBase = collision.GetComponent<ProjectileBase>();
            if (projectileBase) {
                Vector3 hitDirection = (transform.position - collision.transform.position).normalized;
                HandleSpawnEffect(hitDirection);
            }
        }
    }

    private void HandleSpawnEffect(Vector3 hitDirection) {
        --numOfHits;

        if (numOfHits <= 0) {
            if (GameManager.Instance != null) {
                GameManager.Instance.AddScore(scoreValue);
            }

            SpawnEffect(fireProjectile.ExplosionItem, transform.position);
            CameraShake.Instance.Shake(destroyShakeForce);

            if (GameManager.Instance != null && enemyStatus != null && !string.IsNullOrEmpty(enemyStatus.UniqueID)) {
                GameManager.Instance.AddDestroyedEnemy(enemyStatus.UniqueID);
            }

            Destroy(gameObject);
            return;
        }

        Vector3 spawnOffset = hitDirection * hitOffset;
        SpawnEffect(fireProjectile.projectileHitItem, transform.position + spawnOffset);
    }

    private void SpawnEffect(PoolItem poolItem, Vector3 spawnPos) {
        if (poolItem == null) return;
        var effect = Pool.instance.Get(poolItem.name);
        if (effect != null) {
            effect.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0);
            SpriteRenderer sr = effect.GetComponent<SpriteRenderer>();
            if (sr != null) {
                sr.flipX = spawnPos.x < transform.position.x;
            }
        }
    }
}
