using UnityEngine;

public class PressTrapController : MonoBehaviour {

    public int killDamage = 9999;


    public LayerMask playerLayer;


    public Transform castOrigin;
    public Vector3 detectionBoxSize = new Vector3(1f, 0.1f, 1f);
    public float detectionDistance = 0.2f;


    public bool showDebugGizmo = true;

    public void CheckForPlayerAndCrush() {
        Vector3 origin = castOrigin != null ? castOrigin.position : transform.position;

        if (Physics.BoxCast(
                origin,
                detectionBoxSize / 2f,
                Vector3.down,
                out RaycastHit hitInfo,
                transform.rotation,
                detectionDistance,
                playerLayer,
                QueryTriggerInteraction.Ignore)) {
            if (hitInfo.collider.CompareTag("Player")) {
                PlayerHealth playerHealth = hitInfo.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null) {
                    playerHealth.TakeDamage(killDamage);
                }
            }
        }
    }

    void OnDrawGizmosSelected() {
        if (!showDebugGizmo) return;
        Vector3 origin = castOrigin != null ? castOrigin.position : transform.position;
        Gizmos.color = Color.red;
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(origin + Vector3.down * detectionDistance / 2f, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(detectionBoxSize.x, detectionDistance, detectionBoxSize.z));
        Gizmos.matrix = originalMatrix;
    }
}
