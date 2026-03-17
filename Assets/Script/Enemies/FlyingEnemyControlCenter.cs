using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(FlyingEnemyPatrol))]
[RequireComponent(typeof(EnemyChase))]
[RequireComponent(typeof(EnemyKnockbackReceiver))]
public class FlyingEnemyControlCenter : MonoBehaviour
{
    private Rigidbody2D rb;

    private FlyingEnemyPatrol patrol;
    private EnemyChase chase;
    private EnemyKnockbackReceiver knockback;

    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool flipSprite = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        patrol = GetComponent<FlyingEnemyPatrol>();
        chase = GetComponent<EnemyChase>();
        knockback = GetComponent<EnemyKnockbackReceiver>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        chase.SetTargetToChase(ref player);
    }

    private void Update()
    {
        if (knockback != null)
            knockback.UpdateKnockbackStatus();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (player == null)
            chase.SetTargetToChase(ref player);

        // Priority 1: knockback
        if (knockback != null && knockback.IsKnockedback)
        {
            rb.linearVelocity = knockback.Velocity;
            UpdateFacing(rb.linearVelocity);
            return;
        }

        Vector2 moveDirection = Vector2.zero;
        float moveSpeed = 0f;

        // Priority 2: chase player
        if (chase.PlayerInChaseRange(transform, player))
        {
            moveDirection = chase.GetChaseDirection(transform, player);
            moveSpeed = chase.ChaseSpeed;
        }
        // Priority 3: patrol
        else
        {
            moveDirection = patrol.GetPatrolDirection(transform);
            moveSpeed = patrol.PatrolSpeed;
        }

        rb.linearVelocity = moveDirection * moveSpeed;
        UpdateFacing(rb.linearVelocity);
    }

    private void UpdateFacing(Vector2 velocity)
    {
        if (!flipSprite) return;
        if (spriteRenderer == null) return;
        if (Mathf.Abs(velocity.x) < 0.05f) return;

        spriteRenderer.flipX = velocity.x > 0f;
    }
}