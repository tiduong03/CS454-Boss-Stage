using UnityEngine;

public class EnemyChasePatrol : MonoBehaviour
{
    [Header("Patrol")]
    public float leftDistance = 2f;
    public float rightDistance = 2f;
    public float patrolSpeed = 3f;

    [Header("Chase")]
    public float chaseRange = 6f;
    public float chaseSpeed = 4f;

    private Rigidbody2D rb;
    [SerializeField] private Transform player;

    private int direction = 1; // 1 = right, -1 = left
    private float spawnPosition;
    private float leftPatrolPosition;
    private float rightPatrolPosition;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        SetPatrolBounds();
        SetTargetToChase();
    }

    void FixedUpdate()
    {
        if (!rb || !player) return;

        if (PlayerInChaseRange()) ChasePlayer();
        else if (EnemyOutOfPatrolBounds()) ReturnToPatrolBounds();
        else Patrol();
    }

    void SetPatrolBounds()
    {
        spawnPosition = transform.position.x;
        leftPatrolPosition = spawnPosition - leftDistance;
        rightPatrolPosition = spawnPosition + rightDistance;
    }

    void SetTargetToChase()
    {
        if (!player)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject) player = playerObject.transform;
        }
    }

    bool PlayerInChaseRange()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        return dist <= chaseRange;
    }

    void ChasePlayer()
    {
        direction = (player.position.x > transform.position.x) ? 1 : -1;

        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
    }

    bool EnemyOutOfPatrolBounds()
    {
        return transform.position.x < leftPatrolPosition || transform.position.x > rightPatrolPosition;
    }

    void ReturnToPatrolBounds()
    {
        if (transform.position.x < leftPatrolPosition) direction = 1;
        else if (transform.position.x > rightPatrolPosition) direction = -1;

        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
    }

    void Patrol()
    {
        rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);

        if (direction == 1 && transform.position.x >= rightPatrolPosition) direction = -1;
        if (direction == -1 && transform.position.x <= leftPatrolPosition) direction = 1;
    }

    void OnDrawGizmosSelected()
    {
        // shows patrol range
        float spawnPoint = transform.position.x;
        Vector3 leftPatrolPoint = new(spawnPoint - leftDistance, transform.position.y, 0);
        Vector3 rightPatrolPoint = new(spawnPoint + rightDistance, transform.position.y, 0);

        Gizmos.DrawLine(leftPatrolPoint, rightPatrolPoint);
    }
}
