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

    // Jump request (so physics happens in FixedUpdate)
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
        // Input + direction
        move.ReadMoveInput();
        move.UpdateFacingDirection();

        // Jump
        if (jump.CanJump(knockback.IsKnockedback, dash.IsDashing))
            jumpRequested = true;

        // Dash
        dash.CanDash(knockback.IsKnockedback);
        dash.UpdateDashTimer();

        //Attack
        attack.TryAttack(move.Direction, knockback.IsKnockedback, dash.IsDashing);

        //Knockback timer
        knockback.UpdateKnockbackStatus();
    }

    void FixedUpdate()
    {
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump.JumpForce);
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

        rb.linearVelocity = new Vector2(move.MoveInput * move.MoveSpeed, rb.linearVelocity.y);
    }
}
