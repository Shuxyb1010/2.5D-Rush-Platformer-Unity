using UnityEngine;

// It's good practice to ensure the AudioSource component exists
[RequireComponent(typeof(AudioSource))]
public class FireProjectile : MonoBehaviour {

    [SerializeField] private Transform projectileStartTransform;
    [SerializeField] private float maxFireCoolDown;
    private float fireCoolDown;

    public enum ProjectileType {
        Single,
        Double,
        Triple,
        TripleSpread,
        QuintripleSpread
    }

    [SerializeField] private ProjectileType projectileType;
    private int numberOfProjectiles;
    private float heightMarginSign = 1.0f;
    [SerializeField] private float maxVerticalPadding;
    [SerializeField] private float maxHorizontialPadding;
    private float verticalPadding;
    private float horizontalPadding;

    private PlayerMovement playermovement;
    private Animator playerAnimator;

    public PoolItem projectileItem;
    public PoolItem projectileHitItem;
    public PoolItem ExplosionItem;

    private Pool pooler;

    // --- Added for Sound ---
    [Header("Audio Settings")] // Optional: Adds a nice header in the Inspector
    [SerializeField] private AudioClip fireSoundClip; // Assign your firing sound effect here in the Inspector
    private AudioSource audioSource; // Reference to the AudioSource component
    // --- End Sound Additions ---

    private void Awake() {
        playermovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<Animator>();
        pooler = GetComponent<Pool>();

        // --- Added for Sound ---
        // Get the AudioSource component attached to this same GameObject
        audioSource = GetComponent<AudioSource>();
        // --- End Sound Additions ---


        fireCoolDown = maxFireCoolDown;

        switch (projectileType) {
            case ProjectileType.Single:
                numberOfProjectiles = 1;
                break;
            case ProjectileType.Double:
                numberOfProjectiles = 2;
                break;
            case ProjectileType.Triple:
                numberOfProjectiles = 3;
                break;
            case ProjectileType.TripleSpread:
                numberOfProjectiles = 3;
                break;
            // NOTE: You were missing QuintripleSpread in your original switch, added it here.
            case ProjectileType.QuintripleSpread:
                numberOfProjectiles = 5; // Assuming 5 projectiles for this type
                break;
        }

        ConstructPoolItem(projectileItem.name, projectileItem.prefab, projectileItem.parent, projectileItem.amount, projectileItem.expandable);
        ConstructPoolItem(projectileHitItem.name, projectileHitItem.prefab, projectileHitItem.parent, projectileHitItem.amount, projectileHitItem.expandable);
        ConstructPoolItem(ExplosionItem.name, ExplosionItem.prefab, ExplosionItem.parent, ExplosionItem.amount, ExplosionItem.expandable);
    }

    private void ConstructPoolItem(string name, GameObject prefab, Transform parent, int amount, bool expandable) {
        PoolItem item = new PoolItem(name, prefab, parent, amount, expandable);
        pooler.items.Add(item);
    }

    private void Update() {
        if (!InputManager.Instance.FireHeld) return;

        Fire(
            projectileStartTransform.position,
            Quaternion.identity,
            playermovement.GetPlayerDirection()
        );
    }

    private void Fire(Vector3 projectilePos, Quaternion projectileRot, Vector2 projectileDirection) {
        fireCoolDown += Time.deltaTime;

        if (fireCoolDown >= maxFireCoolDown) {
            fireCoolDown = 0.0f;

            // --- Added for Sound ---
            // Play the sound effect if the clip is assigned
            if (fireSoundClip != null && audioSource != null) {
                // Use PlayOneShot for sound effects - allows overlapping sounds if firing fast
                audioSource.PlayOneShot(fireSoundClip);
            } else {
                // Optional: Warn if the sound clip wasn't assigned in the inspector
                if (fireSoundClip == null) Debug.LogWarning("Fire Sound Clip is not assigned on " + gameObject.name);
                if (audioSource == null) Debug.LogWarning("AudioSource component is missing on " + gameObject.name);
            }
            // --- End Sound Additions ---


            if (projectileType == ProjectileType.Double || projectileType == ProjectileType.Triple) {
                horizontalPadding = Mathf.Clamp(maxHorizontialPadding, 0, 1);
            } else if (projectileType == ProjectileType.TripleSpread || projectileType == ProjectileType.QuintripleSpread) {
                verticalPadding = Mathf.Clamp(maxVerticalPadding, 0, 1);
                heightMarginSign = 1.0f;
            }

            for (int i = 0; i < numberOfProjectiles; i++) {
                // --- Your existing projectile spawning logic remains unchanged ---
                if (projectileType == ProjectileType.Double || projectileType == ProjectileType.Triple) {
                    var projectile = Pool.instance.Get(projectileItem.name);
                    Vector3 offset = new Vector3(
                        i * horizontalPadding * playermovement.GetPlayerDirection().x,
                        0.0f,
                        0.0f
                    );
                    projectile.transform.SetPositionAndRotation(projectilePos + offset, projectileRot);
                    projectile.GetComponent<ProjectileBase>().Launch(projectileDirection);
                } else { // Handles Single, TripleSpread, QuintripleSpread
                    if (i == 0) {
                        var projectile = Pool.instance.Get(projectileItem.name);
                        projectile.transform.SetPositionAndRotation(projectilePos, projectileRot);
                        projectile.GetComponent<ProjectileBase>().Launch(projectileDirection);
                    } else { // Handles spread for TripleSpread and QuintripleSpread
                             // This spread logic might need adjustment for QuintripleSpread if you want more angles
                        if (i >= 3) verticalPadding = maxVerticalPadding * 2.0f; // Original logic

                        var projectile = Pool.instance.Get(projectileItem.name);
                        projectile.transform.SetPositionAndRotation(projectilePos, projectileRot);
                        heightMarginSign *= -1.0f;

                        Vector3 spreadDirection = projectileDirection +
                            new Vector2(0.0f, verticalPadding * heightMarginSign);
                        projectile.GetComponent<ProjectileBase>().Launch(spreadDirection);
                    }
                }
                // --- End existing logic ---
            }
        }
    }
}