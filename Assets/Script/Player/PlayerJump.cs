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

    [Header("Wall Checks")]
    [SerializeField] private Transform wallCheckTop;
    [SerializeField] private Transform wallCheckMid;
    [SerializeField] private Transform wallCheckBottom;
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Slide")]
    [SerializeField] private float wallSlideSpeed = 2f;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpXForce = 7f;
    [SerializeField] private float wallJumpYForce = 12f;
    [SerializeField] private float wallCurveJumpTime = 0.12f;

    [Header("Ground Corner Jump")]
    [SerializeField] private float groundCornerJumpAwayX = 0f;
    [SerializeField] private float groundCornerControlLockTime = 0.05f;

    private Rigidbody2D rb;

    private Vector3 wallCheckTopStartLocalPos;
    private Vector3 wallCheckMidStartLocalPos;
    private Vector3 wallCheckBottomStartLocalPos;

    private int facingDirection = 1; // 1 = right, -1 = left
    private float moveInput;
    private float wallCurveJumpTimer;

    private bool hasStoredWallJump;
    private int storedWallSide;

    public bool IsGrounded { get; private set; }
    public bool IsTouchingWall { get; private set; }
    public bool IsWallSliding { get; private set; }
    public bool IsWallJumping => wallCurveJumpTimer > 0f;

    // -1 = wall on left, +1 = wall on right
    public int WallSide { get; private set; }

    public float ForcedWallJumpX { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (wallCheckTop != null)
            wallCheckTopStartLocalPos = wallCheckTop.localPosition;

        if (wallCheckMid != null)
            wallCheckMidStartLocalPos = wallCheckMid.localPosition;

        if (wallCheckBottom != null)
            wallCheckBottomStartLocalPos = wallCheckBottom.localPosition;
    }

    public void UpdateJumpState(float horizontalInput)
    {
        moveInput = horizontalInput;

        UpdateFacingDirection(horizontalInput);
        UpdateWallCheckPositions();

        CheckGround();
        CheckWall();
        UpdateStoredWallJump();
        UpdateWallJumpLock();

        IsWallSliding =
            !IsGrounded &&
            IsTouchingWall &&
            !IsWallJumping &&
            rb.linearVelocity.y <= 0.1f;
    }

    public bool CanJump(bool isKnockedback, bool isDashing)
    {
        if (isKnockedback || isDashing)
            return false;

        if (!Input.GetButtonDown("Jump"))
            return false;

        return IsGrounded || IsTouchingWall || hasStoredWallJump;
    }

    public Vector2 GetJumpVelocity(float currentX)
    {
        int jumpWallSide = GetWallSideForJump();

        if (!IsGrounded && jumpWallSide != 0)
            return GetWallJumpVelocity(jumpWallSide);

        if (IsPressingIntoWallOnGround())
            return GetGroundCornerJumpVelocity();

        return new Vector2(currentX, jumpForce);
    }

    public void ApplyWallSlide()
    {
        if (!IsWallSliding || IsWallJumping)
            return;

        float clampedY = Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedY);
    }

    private void UpdateFacingDirection(float horizontalInput)
    {
        if (horizontalInput > 0.01f)
            facingDirection = 1;
        else if (horizontalInput < -0.01f)
            facingDirection = -1;
    }

    private void UpdateWallCheckPositions()
    {
        SetWallCheckPosition(wallCheckTop, wallCheckTopStartLocalPos);
        SetWallCheckPosition(wallCheckMid, wallCheckMidStartLocalPos);
        SetWallCheckPosition(wallCheckBottom, wallCheckBottomStartLocalPos);
    }

    private void SetWallCheckPosition(Transform check, Vector3 startLocalPos)
    {
        if (check == null)
            return;

        Vector3 position = startLocalPos;
        position.x = Mathf.Abs(startLocalPos.x) * facingDirection;
        check.localPosition = position;
    }

    private void UpdateStoredWallJump()
    {
        if (IsTouchingWall)
        {
            hasStoredWallJump = true;
            storedWallSide = WallSide;
        }

        if (IsGrounded)
        {
            hasStoredWallJump = false;
            storedWallSide = 0;
        }
    }

    private void UpdateWallJumpLock()
    {
        if (wallCurveJumpTimer <= 0f)
            return;

        wallCurveJumpTimer -= Time.deltaTime;

        if (wallCurveJumpTimer <= 0f)
        {
            wallCurveJumpTimer = 0f;
            ForcedWallJumpX = 0f;
        }
    }

    private int GetWallSideForJump()
    {
        if (IsTouchingWall)
            return WallSide;

        if (hasStoredWallJump)
            return storedWallSide;

        return 0;
    }

    private Vector2 GetWallJumpVelocity(int wallSide)
    {
        int jumpDirection = -wallSide;

        ForcedWallJumpX = jumpDirection * wallJumpXForce;
        wallCurveJumpTimer = wallCurveJumpTime;

        hasStoredWallJump = false;
        storedWallSide = 0;

        return new Vector2(ForcedWallJumpX, wallJumpYForce);
    }

    private bool IsPressingIntoWallOnGround()
    {
        if (!IsGrounded || !IsTouchingWall)
            return false;

        bool pressingRightIntoWall = WallSide == 1 && moveInput > 0.01f;
        bool pressingLeftIntoWall = WallSide == -1 && moveInput < -0.01f;

        return pressingRightIntoWall || pressingLeftIntoWall;
    }

    private Vector2 GetGroundCornerJumpVelocity()
    {
        ForcedWallJumpX = -WallSide * groundCornerJumpAwayX;
        wallCurveJumpTimer = groundCornerControlLockTime;

        return new Vector2(ForcedWallJumpX, jumpForce);
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
        bool hasAnyWallCheck =
            wallCheckTop != null ||
            wallCheckMid != null ||
            wallCheckBottom != null;

        if (!hasAnyWallCheck)
        {
            IsTouchingWall = false;
            WallSide = 0;
            return;
        }

        Vector2 castDirection = facingDirection == 1 ? Vector2.right : Vector2.left;

        bool topHit = CastWallRay(wallCheckTop, castDirection);
        bool midHit = CastWallRay(wallCheckMid, castDirection);
        bool bottomHit = CastWallRay(wallCheckBottom, castDirection);

        IsTouchingWall = topHit || midHit || bottomHit;
        WallSide = IsTouchingWall ? facingDirection : 0;
    }

    private bool CastWallRay(Transform checkPoint, Vector2 castDirection)
    {
        if (checkPoint == null)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            checkPoint.position,
            castDirection,
            wallCheckDistance,
            wallLayer
        );

        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Vector3 direction = Application.isPlaying
            ? (facingDirection == 1 ? Vector3.right : Vector3.left)
            : Vector3.right;

        DrawWallGizmo(wallCheckTop, direction);
        DrawWallGizmo(wallCheckMid, direction);
        DrawWallGizmo(wallCheckBottom, direction);
    }

    private void DrawWallGizmo(Transform checkPoint, Vector3 direction)
    {
        if (checkPoint == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            checkPoint.position,
            checkPoint.position + direction * wallCheckDistance
        );
    }
}