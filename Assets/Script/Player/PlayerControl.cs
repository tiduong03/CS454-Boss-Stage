using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;

    // Input/state
    private float moveInput;
    private bool isGrounded;
    private int direction = 1; // 1 = right, -1 = left

    // Dash state
    private bool isDashing;
    private float dashTimeLeft;
    private float lastDashTime = -999f;

    // Knockback state
    private bool isKnockedBack;
    private float knockbackTimeLeft;
    private Vector2 knockbackVelocity;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ReadInput();
        UpdateFacingDirection();
        UpdateIsGrounded();

        TryJump();
        TryDash();
        UpdateDashTimer();

        UpdateKnockbackTimer();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    // Input + state updates
    void ReadInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    void UpdateFacingDirection()
    {
        if (moveInput > 0) direction = 1;
        else if (moveInput < 0) direction = -1;
    }

    void UpdateIsGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }



    // Actions
    void TryJump()
    {
        if (isDashing) return;
        if (isKnockedBack) return;
        if (!Input.GetButtonDown("Jump")) return;
        if (!isGrounded) return;

        Jump();
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void TryDash()
    {
        if (isKnockedBack) return;
        if (!Input.GetKeyDown(KeyCode.LeftShift)) return;
        if (!CanDash()) return;

        StartDash();
    }

    bool CanDash()
    {
        return Time.time >= lastDashTime + dashCooldown && !isDashing;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;
    }

    void UpdateDashTimer()
    {
        if (!isDashing) return;

        dashTimeLeft -= Time.deltaTime;
        
        if (dashTimeLeft <= 0)
            EndDash();
    }

    void EndDash()
    {
        isDashing = false;
    }

    void UpdateKnockbackTimer()
    {
        if (!isKnockedBack) return;

        knockbackTimeLeft -= Time.deltaTime;

        if (knockbackTimeLeft <= 0f)
            isKnockedBack = false;
    }

    // Called by enemies when they hit the player.
    public void ApplyKnockback(Vector2 velocity, float duration)
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();

        // Cancel dash
        isDashing = false;
        dashTimeLeft = 0f;

        isKnockedBack = true;
        knockbackTimeLeft = duration;
        knockbackVelocity = velocity;

        rb.linearVelocity = velocity;
    }

    // Physics movement
    void ApplyMovement()
    {
        if (isKnockedBack)
        {
            rb.linearVelocity = new Vector2(knockbackVelocity.x, rb.linearVelocity.y);
            return;
        }

        if (isDashing)
        {
            rb.linearVelocity = new Vector2(direction * dashSpeed, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}