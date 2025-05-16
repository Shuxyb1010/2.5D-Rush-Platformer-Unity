using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [Header("References")]
    public PlayerMovementStats MoveStats;
    private PlayerHealth playerHealth;

    private Rigidbody _rb;
    private Collider _collider;
    private Vector3 _moveVelocity;
    private bool _isFacingRight;
    private bool _isGrounded;
    private bool _bumpedHead;

    public float VerticalVelocity { get; private set; }
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    private float jumpBufferTimer;
    private bool jumpReleasedDuringBuffer;

    private float _coyoteTimer;

    private Animator animator;

    private void Awake() {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start() {
    }

    private void Update() {
        if (Time.timeScale < 0.1f) return;
        CountTimers();
        JumpChecks();
        float horizontalSpeed = Mathf.Abs(InputManager.Instance.Movement.x);
        animator.SetFloat("Speed", horizontalSpeed);
        animator.SetBool("IsGrounded", _isGrounded);
        if (InputManager.Instance.JumpPressed) {
            animator.SetTrigger("Jump");
        }
        if (InputManager.Instance.FireHeld) {
            animator.SetTrigger("Shoot");
        }
    }

    private void FixedUpdate() {
        CollisionCheck();
        Jump();
        if (_isGrounded) {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Instance.Movement);
        } else {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Instance.Movement);
        }
    }

    private void Move(float acceleration, float deceleration, Vector3 moveInput) {
        if (moveInput != Vector3.zero) {
            TurnCheck(moveInput);
            Vector3 targetVelocity = Vector3.zero;
            if (InputManager.Instance.RunHeld) {
                targetVelocity = new Vector3(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            } else {
                targetVelocity = new Vector3(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            }
            _moveVelocity = Vector3.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector3(_moveVelocity.x, _rb.linearVelocity.y);
        } else {
            _moveVelocity = Vector3.Lerp(_moveVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector3(_moveVelocity.x, _rb.linearVelocity.y);
        }
    }

    private void TurnCheck(Vector3 moveInput) {
        if (_isFacingRight && moveInput.x < 0) {
            Turn(false);
        } else if (!_isFacingRight && moveInput.x > 0) {
            Turn(true);
        }
    }

    private void Turn(bool turnRight) {
        if (turnRight) {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        } else {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    public Vector2 GetPlayerDirection() { return _isFacingRight ? Vector2.right : Vector2.left; }

    private void IsGrounded() {
        Vector3 origin = _collider.bounds.center;
        origin.y = _collider.bounds.min.y + 0.01f;
        float checkDistance = MoveStats.groundCheckDistance + 0.05f;
        float width = _collider.bounds.extents.x * 0.9f;
        bool center = Physics.Raycast(origin, Vector3.down, out _, checkDistance, MoveStats.GroundLayer);
        bool left = Physics.Raycast(origin - new Vector3(width, 0, 0), Vector3.down, checkDistance, MoveStats.GroundLayer);
        bool right = Physics.Raycast(origin + new Vector3(width, 0, 0), Vector3.down, checkDistance, MoveStats.GroundLayer);
        _isGrounded = center || left || right;
    }

    private void JumpChecks() {
        if (_isGrounded && !isJumping) {
            _numberOfJumpsUsed = 0;
        }
        if (InputManager.Instance.JumpPressed) {
            jumpBufferTimer = MoveStats.JumpBufferTime;
            jumpReleasedDuringBuffer = false;
        }
        if (InputManager.Instance.JumpReleased) {
            if (jumpBufferTimer > 0f) {
                jumpReleasedDuringBuffer = true;
            }
            if (isJumping && VerticalVelocity > 0f) {
                if (isPastApexThreshold) {
                    isPastApexThreshold = false;
                    isFastFalling = true;
                    fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                } else {
                    isFastFalling = true;
                    fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }
        if (jumpBufferTimer > 0f && !isJumping && (_isGrounded || _coyoteTimer > 0f)) {
            InitiateJump(1);
            _coyoteTimer = 0f;
            if (jumpReleasedDuringBuffer) {
                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;
            }
        } else if (jumpBufferTimer > 0f && (_numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)) {
            InitiateJump(1);
            isFastFalling = false;
        }
    }

    private void InitiateJump(int jumpCount) {
        isJumping = true;
        isFalling = false;
        jumpBufferTimer = 0f;
        _numberOfJumpsUsed += jumpCount;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
        isPastApexThreshold = false;
        isFastFalling = false;
    }

    private void Jump() {
        if (_isGrounded && VerticalVelocity <= 0f) {
            isJumping = false;
            isFalling = false;
            isFastFalling = false;
            fastFallTime = 0f;
            isPastApexThreshold = false;
            VerticalVelocity = 0f;
            return;
        }
        if (_bumpedHead && VerticalVelocity > 0f) {
            VerticalVelocity = 0f;
            isFastFalling = true;
            isFalling = true;
        }
        if (isJumping) {
            if (VerticalVelocity >= 0f) {
                apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);
                if (apexPoint > MoveStats.ApexThreshold) {
                    if (!isPastApexThreshold) {
                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }
                    if (timePastApexThreshold < MoveStats.ApexHangTime) {
                        VerticalVelocity = 0f;
                        timePastApexThreshold += Time.fixedDeltaTime;
                    } else {
                        VerticalVelocity = -0.01f;
                        isFalling = true;
                    }
                } else {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                }
            } else {
                isFalling = true;
                float gravityMultiplier = isFastFalling ? MoveStats.GravityOnReleaseMultiplier : 1f;
                VerticalVelocity += MoveStats.Gravity * gravityMultiplier * Time.fixedDeltaTime;
            }
        }
        if (isFastFalling) {
            if (fastFallTime >= MoveStats.TimeForUpwardsCancel) {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            } else {
                VerticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, fastFallTime / MoveStats.TimeForUpwardsCancel);
                fastFallTime += Time.fixedDeltaTime;
            }
        }
        if (!_isGrounded && !isJumping) {
            isFalling = true;
            float gravityMultiplier = isFastFalling ? MoveStats.GravityOnReleaseMultiplier : 1f;
            VerticalVelocity += MoveStats.Gravity * gravityMultiplier * Time.fixedDeltaTime;
        }
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, MoveStats.InitialJumpVelocity);
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, VerticalVelocity);
    }

    private void BumpedHead() {
        Vector3 rayOrigin = _collider.bounds.center;
        rayOrigin.y = _collider.bounds.max.y;
        float width = _collider.bounds.extents.x * MoveStats.HeadWidth;
        bool left = Physics.Raycast(rayOrigin - new Vector3(width, 0, 0),
            Vector3.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        bool center = Physics.Raycast(rayOrigin,
            Vector3.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        bool right = Physics.Raycast(rayOrigin + new Vector3(width, 0, 0),
            Vector3.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        _bumpedHead = left || center || right;
    }

    private void CountTimers() {
        jumpBufferTimer -= Time.deltaTime;
        _coyoteTimer = _isGrounded ? MoveStats.JumpCoyoteTime :
            Mathf.Max(_coyoteTimer - Time.deltaTime, 0);
    }

    private void CollisionCheck() {
        IsGrounded();
        BumpedHead();
    }

    public void Save(ref SaveSystem.PlayerSaveData data) {
        data.position = transform.position;
        data.isFacingRight = _isFacingRight;
        if (playerHealth != null) {
            data.currentHealth = playerHealth.GetCurrentHealth();
        }
    }

    public void Load(SaveSystem.PlayerSaveData data) {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_rb == null) { return; }
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        VerticalVelocity = 0f;
        isJumping = false;
        isFalling = false;
        isFastFalling = false;
        _rb.position = data.position;
        if (_isFacingRight != data.isFacingRight) {
            Turn(data.isFacingRight);
        }
        if (playerHealth != null) {
            playerHealth.SetHealth(data.currentHealth);
        }
        CollisionCheck();
    }
}

[System.Serializable]
public class PlayerSaveData {
    public Vector3 Position;
}
