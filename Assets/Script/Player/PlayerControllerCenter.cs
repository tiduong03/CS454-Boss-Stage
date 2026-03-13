using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerBasicMovement))]
[RequireComponent(typeof(PlayerJump))]
[RequireComponent(typeof(PlayerDash))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerKnockbackReceiver))]

public class PlayerControlCenter : MonoBehaviour
{
    private Rigidbody2D rb;

    private PlayerBasicMovement move;
    private PlayerJump jump;
    private PlayerDash dash;
    private PlayerAttack attack;
    private PlayerKnockbackReceiver knockback;

    private bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        move = GetComponent<PlayerBasicMovement>();
        jump = GetComponent<PlayerJump>();
        dash = GetComponent<PlayerDash>();
        attack = GetComponent<PlayerAttack>();
        knockback = GetComponent<PlayerKnockbackReceiver>();
    }

    void Update()
    {
        move.ReadMoveInput();
        move.UpdateFacingDirection();

        // Update jump state (ground / wall checks / timers)
        jump.Tick(move.MoveInput);

        if (jump.CanJump(knockback.IsKnockedback, dash.IsDashing))
            jumpRequested = true;

        dash.CanDash(knockback.IsKnockedback);
        dash.UpdateDashTimer();

        attack.TryAttack(move.Direction, knockback.IsKnockedback, dash.IsDashing);

        knockback.UpdateKnockbackStatus();
    }

    void FixedUpdate()
    {
        if (jumpRequested)
        {
            rb.linearVelocity = jump.GetJumpVelocity(rb.linearVelocity.x);
            jumpRequested = false;
        }

        if (knockback.IsKnockedback)
        {
            rb.linearVelocity = knockback.Velocity;
            return;
        }

        if (dash.IsDashing)
        {
            rb.linearVelocity = new Vector2(move.Direction * dash.DashSpeed, rb.linearVelocity.y);
            return;
        }

        jump.ApplyWallSlide();

        float xVelocity = move.MoveInput * move.MoveSpeed;

        // If sliding on a wall and pressing into it, don't keep pushing into the wall
        if (jump.IsWallSliding)
        {
            bool pressingIntoWall = (jump.WallSide == 1 && move.MoveInput > 0f) || (jump.WallSide == -1 && move.MoveInput < 0f);

            if (pressingIntoWall)
                xVelocity = 0f;
        }

        // Briefly lock horizontal movement after wall jump
        if (jump.IsWallJumping)
            xVelocity = jump.ForcedWallJumpX;

        rb.linearVelocity = new Vector2(xVelocity, rb.linearVelocity.y);
    }
}