using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.6f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    // Input/state
    private float moveInput;
    private bool isGrounded;

    // Facing + dash state
    private int facingDir = 1; // 1 = right, -1 = left
    private bool isDashing;
    private float dashTimeLeft;
    private float lastDashTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ReadInput();
        UpdateFacingDirection();
        UpdateGrounded();

        TryJump();
        TryDash();
        UpdateDashTimer();
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
        if (moveInput > 0) facingDir = 1;
        else if (moveInput < 0) facingDir = -1;
    }

    void UpdateGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }



    // Actions
    void TryJump()
    {
        if (isDashing) return;
        if (!Input.GetButtonDown("Jump")) return;
        if (!isGrounded) return;

        Jump();
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    void TryDash()
    {
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



    // Physics movement
    void ApplyMovement()
    {
        if (isDashing)
        {
            rb.velocity = new Vector2(facingDir * dashSpeed, rb.velocity.y);
            return;
        }

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }
}