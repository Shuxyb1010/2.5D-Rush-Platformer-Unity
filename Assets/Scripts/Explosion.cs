using UnityEngine;

public class Explosion : MonoBehaviour {
    [SerializeField] private int maxNumOfHits;
    [SerializeField] private FireProjectile fireProjectile;
    [SerializeField] private float hitOffset = -1.5f;
    private int numOfHits;
    private ProjectileBase projectileBase;
    [SerializeField] private float destroyShakeForce = 0.3f;
    private string _blockID;

    private void Awake() {
        GenerateBlockID();
    }

    private void GenerateBlockID() {
        Vector3 pos = transform.position;
        _blockID = $"{gameObject.scene.name}_Block_{pos.x:F2}_{pos.y:F2}_{pos.z:F2}";
    }

    private void Start() {
        numOfHits = maxNumOfHits;
        if (GameManager.Instance.IsBlockDestroyed(_blockID)) {
            Destroy(gameObject);
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

        if (numOfHits == 0) {
 
            SpawnEffect(fireProjectile.ExplosionItem, transform.position);

            CameraShake.Instance.Shake(destroyShakeForce);
            GameManager.Instance.AddDestroyedBlock(_blockID);
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