using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(EnemyChase))]
[RequireComponent(typeof(EnemyKnockbackReceiver))]

public class EnemyControlCenter : MonoBehaviour
{
    private Rigidbody2D rb;

    private EnemyPatrol patrol;
    private EnemyChase chase;
    private EnemyKnockbackReceiver knockback;

    [SerializeField] private Transform player;

    private int direction = 1; // 1 right, -1 left

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        patrol = GetComponent<EnemyPatrol>();
        chase = GetComponent<EnemyChase>();
        knockback = GetComponent<EnemyKnockbackReceiver>();
    }

    void Start()
    {
        patrol.SetPatrolBounds(transform);
        chase.SetTargetToChase(ref player);
    }

    void Update()
    {
        //Knockback timer
        knockback.UpdateKnockbackStatus();
    }

    void FixedUpdate()
    {
        if (!rb) return;

        // Priority 1: knockback
        if (knockback.IsKnockedback)
        {
            rb.linearVelocity = knockback.Velocity;
            return;
        }

        // Priority 2: chase
        if (chase.PlayerInChaseRange(transform, player))
        {
            direction = chase.GetChaseDirection(transform, player);
            rb.linearVelocity = new Vector2(direction * chase.ChaseSpeed, rb.linearVelocity.y);
        }

        // Priority 3: return to patrol bounds
        else if (patrol.EnemyOutOfPatrolBounds(transform))
        {
            direction = patrol.GetReturnDirection(transform);
            rb.linearVelocity = new Vector2(direction * chase.ChaseSpeed, rb.linearVelocity.y);
        }

        // Priority 4: patrol
        else
        {
            rb.linearVelocity = new Vector2(direction * patrol.PatrolSpeed, rb.linearVelocity.y);
            patrol.UpdatePatrolDirection(ref direction, transform);
        }

        // prevent getting bumped upward
        if (rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    }
}