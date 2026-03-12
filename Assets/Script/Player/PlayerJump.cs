using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    public float JumpForce => jumpForce;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Slide")]
    [SerializeField] private float wallSlideSpeed = 2f;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpXForce = 7f;
    [SerializeField] private float wallJumpYForce = 12f;
    [SerializeField] private float wallJumpControlLockTime = 0.12f;

    private Rigidbody2D rb;
    private Vector3 wallCheckStartLocalPos;
    private int facingDirection = 1; // 1 = right, -1 = left

    public bool IsGrounded { get; private set; }
    public bool IsTouchingWall { get; private set; }
    public bool IsWallSliding { get; private set; }
    public bool IsWallJumping => wallJumpLockCounter > 0f;

    // -1 = wall on left, +1 = wall on right
    public int WallSide { get; private set; }

    public float ForcedWallJumpX { get; private set; }

    private float wallJumpLockCounter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (wallCheck != null)
            wallCheckStartLocalPos = wallCheck.localPosition;
    }

    public void Tick(float moveInput)
    {
        if (moveInput > 0.01f)
            facingDirection = 1;
        else if (moveInput < -0.01f)
            facingDirection = -1;

        UpdateWallCheckPosition();

        CheckGround();
        CheckWall();

        if (wallJumpLockCounter > 0f)
        {
            wallJumpLockCounter -= Time.deltaTime;

            if (wallJumpLockCounter <= 0f)
            {
                wallJumpLockCounter = 0f;
                ForcedWallJumpX = 0f;
            }
        }

        IsWallSliding = !IsGrounded
                        && IsTouchingWall
                        && !IsWallJumping
                        && rb.linearVelocity.y <= 0.1f;
    }

    private void UpdateWallCheckPosition()
    {
        if (wallCheck == null)
            return;

        Vector3 pos = wallCheckStartLocalPos;
        pos.x = Mathf.Abs(wallCheckStartLocalPos.x) * facingDirection;
        wallCheck.localPosition = pos;
    }

    public bool CanJump(bool isKnockedback, bool isDashing)
    {
        if (isKnockedback || isDashing)
            return false;

        if (!Input.GetButtonDown("Jump"))
            return false;

        return IsGrounded || IsTouchingWall;
    }

    public Vector2 GetJumpVelocity(float currentX)
    {
        if (IsTouchingWall && !IsGrounded)
        {
            int jumpDirection = -WallSide;
            ForcedWallJumpX = jumpDirection * wallJumpXForce;
            wallJumpLockCounter = wallJumpControlLockTime;

            return new Vector2(ForcedWallJumpX, wallJumpYForce);
        }

        return new Vector2(currentX, jumpForce);
    }

    public void ApplyWallSlide()
    {
        if (!IsWallSliding || IsWallJumping)
            return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            IsGrounded = false;
            return;
        }

        IsGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void CheckWall()
    {
        if (wallCheck == null)
        {
            IsTouchingWall = false;
            WallSide = 0;
            return;
        }

        Vector2 castDirection = facingDirection == 1 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            castDirection,
            wallCheckDistance,
            wallLayer
        );

        IsTouchingWall = hit.collider != null;
        WallSide = IsTouchingWall ? facingDirection : 0;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;

            Vector3 dir = Application.isPlaying
                ? (facingDirection == 1 ? Vector3.right : Vector3.left)
                : Vector3.right;

            Gizmos.DrawLine(wallCheck.position, wallCheck.position + dir * wallCheckDistance);
        }
    }
}