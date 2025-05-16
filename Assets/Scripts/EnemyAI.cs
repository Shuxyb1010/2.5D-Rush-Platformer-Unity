using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyWalker : MonoBehaviour {
    private enum EnemyState { Patrolling, Chasing, Attacking }
    private EnemyState currentState;

  
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    private int moveDirection = 1;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 2f;
    public float attackDuration = 0.5f;

    public Transform attackPoint;

    public float attackRadius = 0.5f;

    public LayerMask playerLayer;
    private bool canAttack = true;

    [Header("Player Detection")]
    public float playerDetectionRange = 8f;
    public float playerLoseRange = 12f;

    private Transform playerTransform;

    public LayerMask groundLayer;
    public float wallCheckDistance = 0.6f;

    public float ledgeCheckHorizontalOffset = 0.5f;
    public Vector3 ledgeCheckBoxSize = new Vector3(0.4f, 0.1f, 0.1f);
    public float ledgeCheckMaxDistance = 1.0f;
    public float jumpDownThreshold = 0.8f;

    
    public float turnCooldown = 0.2f;
    private bool canCheckEnv = true;

    public float facingDeadZone = 0.1f;


    public bool showGizmos = true;

    private Rigidbody rb;

    private Collider enemyCollider;
    private Animator animator;
    private float currentSpeed;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();

        rb.constraints = RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) {
            playerTransform = playerObject.transform;
        }

        currentState = EnemyState.Patrolling;
        currentSpeed = patrolSpeed;
    }

    void Update() {
        UpdateAnimator();
    }

    void FixedUpdate() {
        if (playerTransform == null && currentState != EnemyState.Patrolling) {
            currentState = EnemyState.Patrolling;
            currentSpeed = patrolSpeed;
            canCheckEnv = true;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        HandleStateTransitions();
        PerformCurrentStateActions();
    }

    void UpdateAnimator() {
        if (animator == null) return;
        float speed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", speed);
    }

    void HandleStateTransitions() {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState) {
            case EnemyState.Patrolling:
                if (distanceToPlayer < playerDetectionRange && HasLineOfSightToPlayer()) {
                    currentState = EnemyState.Chasing;
                    currentSpeed = chaseSpeed;
                }
                break;

            case EnemyState.Chasing:
                if (distanceToPlayer > playerLoseRange) {
                    currentState = EnemyState.Patrolling;
                    currentSpeed = patrolSpeed;
                    canCheckEnv = true;
                } else if (distanceToPlayer <= attackRange && canAttack && HasLineOfSightToPlayer()) {
                    currentState = EnemyState.Attacking;
                }
                break;

            case EnemyState.Attacking:
                break;
        }
    }

    void PerformCurrentStateActions() {
        switch (currentState) {
            case EnemyState.Patrolling:
                PerformPatrol();
                break;
            case EnemyState.Chasing:
                PerformChase();
                break;
            case EnemyState.Attacking:
                PerformAttack();
                break;
        }
    }

    void PerformAttack() {
        if (playerTransform != null) {
            float horizontalDistance = playerTransform.position.x - transform.position.x;
            if (Mathf.Abs(horizontalDistance) > facingDeadZone) {
                int directionToPlayerX = (int)Mathf.Sign(horizontalDistance);
                FaceDirection(directionToPlayerX);
            }
        }

        if (canAttack) {
            StartCoroutine(AttackSequence());
        }
    }

    IEnumerator AttackSequence() {
        canAttack = false;
        var originalConstraints = rb.constraints;
        Vector3 entryVelocity = rb.linearVelocity;

        rb.constraints |= RigidbodyConstraints.FreezePositionX;
        rb.linearVelocity = new Vector3(0, entryVelocity.y, 0);
        rb.angularVelocity = Vector3.zero;

        animator.SetTrigger("Attack");

        float hitTime = attackDuration * 0.2f;
        yield return new WaitForSeconds(hitTime);

        if (attackPoint != null && playerTransform != null) {
            Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer);
            foreach (Collider player in hitPlayers) {
                if (player.CompareTag("Player")) {
                    Debug.Log("Enemy Attack Hit Player");
                    player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
                    break;
                }
            }
        }

        float remainingTime = attackDuration - hitTime;
        if (remainingTime > 0) {
            yield return new WaitForSeconds(remainingTime);
        }

        rb.constraints = originalConstraints;

        if (playerTransform != null) {
            currentState = EnemyState.Chasing;
            currentSpeed = chaseSpeed;
        } else {
            currentState = EnemyState.Patrolling;
            currentSpeed = patrolSpeed;
            canCheckEnv = true;
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void PerformPatrol() {
        if (canCheckEnv) {
            bool isBlockedOrTurned = CheckEnvironment(false);
            if (isBlockedOrTurned) {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0f);
                return;
            }
        }
        rb.linearVelocity = new Vector3(moveDirection * currentSpeed, rb.linearVelocity.y, 0f);
    }

    void PerformChase() {
        if (playerTransform == null) {
            currentState = EnemyState.Patrolling;
            currentSpeed = patrolSpeed;
            canCheckEnv = true;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        float horizontalDistance = playerTransform.position.x - transform.position.x;
        if (Mathf.Abs(horizontalDistance) > facingDeadZone) {
            int directionToPlayerX = (int)Mathf.Sign(horizontalDistance);
            FaceDirection(directionToPlayerX);
        }

        bool isPathBlocked = CheckEnvironment(true);
        if (isPathBlocked) {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0f);
        } else {
            rb.linearVelocity = new Vector3(moveDirection * currentSpeed, rb.linearVelocity.y, 0f);
        }
    }

    bool CheckEnvironment(bool allowLedgeJump) {
        float colliderExtentX = enemyCollider.bounds.extents.x;
        Vector3 wallCheckRayOrigin = transform.position + (transform.right * (colliderExtentX + 0.05f));
        bool isWallAhead = Physics.Raycast(wallCheckRayOrigin, transform.right, wallCheckDistance, groundLayer);

        if (isWallAhead) {
            if (currentState == EnemyState.Patrolling && canCheckEnv) {
                Turn();
            }
            return true;
        }

        Vector3 boxCastCenter = transform.position + (transform.right * ledgeCheckHorizontalOffset) + (Vector3.up * (ledgeCheckBoxSize.y / 2f));
        bool isGroundAhead = Physics.BoxCast(boxCastCenter, ledgeCheckBoxSize / 2f, Vector3.down, out RaycastHit _, transform.rotation, ledgeCheckMaxDistance, groundLayer);

        if (!isGroundAhead) {
            if (currentState == EnemyState.Chasing && allowLedgeJump) {
                if (playerTransform == null) return true;
                bool isPlayerBelow = playerTransform.position.y < transform.position.y - jumpDownThreshold;
                return !isPlayerBelow;
            } else if (currentState == EnemyState.Patrolling && canCheckEnv) {
                Turn();
                return true;
            } else {
                return true;
            }
        }

        return false;
    }

    void Turn() {
        if (!canCheckEnv) return;
        RotateEnemy();
        canCheckEnv = false;
        StartCoroutine(EnvironmentCheckCooldownRoutine());
    }

    void RotateEnemy() {
        moveDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }

    void FaceDirection(int direction) {
        if (direction == 0) return;
        if (moveDirection != direction) {
            RotateEnemy();
        }
    }

    IEnumerator EnvironmentCheckCooldownRoutine() {
        yield return new WaitForSeconds(turnCooldown);
        canCheckEnv = true;
    }

    bool HasLineOfSightToPlayer() {
        if (playerTransform == null) return false;
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        if (directionToPlayer == Vector3.zero) return true;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        int layerMask = ~(1 << gameObject.layer | 1 << LayerMask.NameToLayer("Ground"));
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, distanceToPlayer, layerMask)) {
            return hit.transform == playerTransform || hit.transform.IsChildOf(playerTransform);
        }

        return true;
    }

    void OnDrawGizmosSelected() {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, playerLoseRange);

        if (attackPoint != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        if (enemyCollider == null) enemyCollider = GetComponent<Collider>();
        if (enemyCollider == null) return;

        Vector3 forwardDir = transform.right * moveDirection;

        Gizmos.color = Color.blue;
        float colliderExtentX = enemyCollider.bounds.extents.x;
        Vector3 wallRayStart = transform.position + (transform.right * (colliderExtentX + 0.05f));
        Gizmos.DrawLine(wallRayStart, wallRayStart + transform.right * wallCheckDistance);

        Gizmos.color = Color.cyan;
        Vector3 boxCastCenter = transform.position +
                                (transform.right * ledgeCheckHorizontalOffset) +
                                (Vector3.up * (ledgeCheckBoxSize.y / 2f));
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(boxCastCenter + Vector3.down * ledgeCheckMaxDistance * 0.5f, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(ledgeCheckBoxSize.x, ledgeCheckMaxDistance, ledgeCheckBoxSize.z));
        Gizmos.matrix = originalMatrix;
        Gizmos.DrawLine(boxCastCenter, boxCastCenter + Vector3.down * 0.1f);

        Gizmos.color = Color.magenta;
        Vector3 thresholdPoint = transform.position + Vector3.down * jumpDownThreshold;
        Gizmos.DrawLine(thresholdPoint + Vector3.left * 0.3f, thresholdPoint + Vector3.right * 0.3f);

        Gizmos.color = Color.green;
        Vector3 deadZoneLeft = transform.position + Vector3.left * facingDeadZone + Vector3.up * 0.1f;
        Vector3 deadZoneRight = transform.position + Vector3.right * facingDeadZone + Vector3.up * 0.1f;
        Gizmos.DrawLine(deadZoneLeft, deadZoneLeft + Vector3.up * 0.5f);
        Gizmos.DrawLine(deadZoneRight, deadZoneRight + Vector3.up * 0.5f);
    }
}
