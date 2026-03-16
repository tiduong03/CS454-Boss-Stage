using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class BossAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform firePoint;
    [SerializeField] private BossProjectile projectilePrefab;

    [Header("Arena Movement")]
    [Tooltip("Drop your platform colliders here. The boss will pick random hover points above them.")]
    [SerializeField] private List<Collider2D> arenaPlatforms = new List<Collider2D>();
    [SerializeField] private float hoverHeightAbovePlatform = 2f;
    [SerializeField] private float platformEdgePadding = 0.5f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float retargetTime = 2f;
    [SerializeField] private float arriveDistance = 0.2f;
    [SerializeField] private float hoverAmplitude = 0.25f;
    [SerializeField] private float hoverFrequency = 2f;
    [SerializeField] private bool facePlayer = true;

    [Header("Summons")]
    [Tooltip("Spawned once when boss HP reaches 50% or lower.")]
    [SerializeField] private GameObject phase1EnemyPrefab;
    [SerializeField] private int phase1SpawnCount = 1;

    [Header("Split Phase (30%)")]
    [Tooltip("Boss prefab to spawn when this boss reaches 30% HP.")]
    [SerializeField] private BossAI splitBossPrefab;
    [SerializeField] private int splitBossCount = 1;
    [SerializeField][Range(0.01f, 1f)] private float splitHpPercent = 0.30f;

    [Tooltip("Optional. If empty, summons / split bosses will spawn near the boss.")]
    [SerializeField] private Transform[] summonPoints;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int projectileDamage = 5;
    [SerializeField] private float spreadAngle = 18f;
    [SerializeField] private int radialProjectileCount = 8;

    private Rigidbody2D rb;
    private EnemyHealth health;

    private Vector2 moveTarget;
    private float retargetTimer;
    private float attackTimer;

    private bool phase1Triggered;
    private bool phase2Triggered;
    private bool canSplit = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        if (!player)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject)
                player = playerObject.transform;
        }

        PickNewMoveTarget();
        retargetTimer = retargetTime;
        attackTimer = attackCooldown;
    }

    private void Update()
    {
        if (health.IsDead)
            return;

        HandlePhaseChanges();
        HandleRetargetTimer();
        HandleAttackTimer();
        UpdateFacing();
    }

    private void FixedUpdate()
    {
        if (health.IsDead)
            return;

        MoveBoss();
    }

    private void HandlePhaseChanges()
    {
        float hpPercent = GetHealthPercent();

        if (!phase1Triggered && hpPercent <= 0.5f)
        {
            phase1Triggered = true;
            //projectileDamage = 2;
            //attackCooldown *= 0.5f;
            //SpawnMinions(phase1EnemyPrefab, phase1SpawnCount);
            //SpawnSplitBosses();
        }

        if (!phase2Triggered && hpPercent <= 0.3f)
        {
            phase2Triggered = true;
        }

        if (canSplit && hpPercent <= splitHpPercent)
        {
            SpawnSplitBosses();
        }
    }

    private void SpawnSplitBosses()
    {
        if (!splitBossPrefab || splitBossCount <= 0)
            return;

        canSplit = false;
        int splitHp = Mathf.CeilToInt(health.maxHP * splitHpPercent);

        for (int i = 0; i < splitBossCount; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition(i);
            BossAI newBoss = Instantiate(splitBossPrefab, spawnPosition, Quaternion.identity);
            newBoss.InitializeSplitBoss(player, splitHp);
        }
    }

    public void InitializeSplitBoss(Transform targetPlayer, int newHp)
    {
        player = targetPlayer;
        canSplit = false;

        // Spawned boss starts already in late phase.
        phase1Triggered = true;
        phase2Triggered = true;

        if (health != null)
            health.SetCurrentHP(newHp);

        PickNewMoveTarget();
        retargetTimer = retargetTime;
        attackTimer = attackCooldown;
    }

    private float GetHealthPercent()
    {
        if (health.maxHP <= 0)
            return 0f;

        return (float)health.CurrentHP / health.maxHP;
    }

    private void HandleRetargetTimer()
    {
        retargetTimer -= Time.deltaTime;

        bool reachedTarget = Vector2.Distance(rb.position, moveTarget) <= arriveDistance;
        if (reachedTarget || retargetTimer <= 0f)
        {
            PickNewMoveTarget();
            retargetTimer = retargetTime;
        }
    }

    private void PickNewMoveTarget()
    {
        if (arenaPlatforms == null || arenaPlatforms.Count == 0)
        {
            moveTarget = rb.position;
            return;
        }

        Collider2D platform = arenaPlatforms[Random.Range(0, arenaPlatforms.Count)];
        if (!platform)
        {
            moveTarget = rb.position;
            return;
        }

        Bounds bounds = platform.bounds;

        float minX = bounds.min.x + platformEdgePadding;
        float maxX = bounds.max.x - platformEdgePadding;

        if (minX > maxX)
        {
            minX = bounds.center.x;
            maxX = bounds.center.x;
        }

        float targetX = Random.Range(minX, maxX);
        float targetY = bounds.max.y + hoverHeightAbovePlatform;

        moveTarget = new Vector2(targetX, targetY);
    }

    private void MoveBoss()
    {
        Vector2 hoverOffset = new Vector2(0f, Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude);
        Vector2 desiredPosition = moveTarget + hoverOffset;
        Vector2 nextPosition = Vector2.MoveTowards(rb.position, desiredPosition, moveSpeed * Time.fixedDeltaTime);

        rb.MovePosition(nextPosition);
    }

    private void HandleAttackTimer()
    {
        if (!player)
            return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f)
            return;

        DoAttack();
        attackTimer = attackCooldown;
    }

    private void DoAttack()
    {
        int unlockedAttackCount = 1;

        if (phase1Triggered)
            unlockedAttackCount++;

        if (phase2Triggered)
            unlockedAttackCount++;

        int attackIndex = Random.Range(0, unlockedAttackCount);

        switch (attackIndex)
        {
            case 0:
                BasicShot();
                break;

            case 1:
                SpreadShot();
                break;

            case 2:
                RadialBurst();
                break;
        }
    }

    private void BasicShot()
    {
        Vector2 direction = GetDirectionToPlayer();
        Shoot(direction);
    }

    private void SpreadShot()
    {
        Vector2 baseDirection = GetDirectionToPlayer();
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        Shoot(AngleToDirection(baseAngle - spreadAngle));
        Shoot(AngleToDirection(baseAngle));
        Shoot(AngleToDirection(baseAngle + spreadAngle));
    }

    private void RadialBurst()
    {
        if (radialProjectileCount < 1)
            return;

        float step = 360f / radialProjectileCount;
        for (int i = 0; i < radialProjectileCount; i++)
        {
            float angle = step * i;
            Shoot(AngleToDirection(angle));
        }
    }

    private Vector2 GetDirectionToPlayer()
    {
        if (!player)
            return Vector2.right;

        Vector2 direction = player.position - transform.position;
        if (direction.sqrMagnitude <= 0.001f)
            return Vector2.right;

        return direction.normalized;
    }

    private Vector2 AngleToDirection(float angleDeg)
    {
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
    }

    private void Shoot(Vector2 direction)
    {
        if (!projectilePrefab || !firePoint)
        {
            Debug.LogWarning($"{name}: Assign a projectile prefab and fire point on BossAI.");
            return;
        }

        BossProjectile projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.Initialize(direction, projectileSpeed, projectileDamage);
    }

    private void SpawnMinions(GameObject enemyPrefab, int amount)
    {
        if (!enemyPrefab || amount <= 0)
            return;

        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition(i);
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private Vector3 GetSpawnPosition(int index)
    {
        if (summonPoints != null && summonPoints.Length > 0)
        {
            Transform point = summonPoints[index % summonPoints.Length];
            if (point)
                return point.position;
        }

        float offsetX = Random.Range(-2f, 2f);
        float offsetY = Random.Range(-1f, 1f);
        return transform.position + new Vector3(offsetX, offsetY, 0f);
    }

    private void UpdateFacing()
    {
        if (!facePlayer || !player)
            return;

        Vector3 scale = transform.localScale;
        float sign = (player.position.x >= transform.position.x) ? 1f : -1f;
        scale.x = Mathf.Abs(scale.x) * sign;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(moveTarget, 0.2f);

        if (summonPoints == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (Transform point in summonPoints)
        {
            if (point)
                Gizmos.DrawWireSphere(point.position, 0.25f);
        }
    }
}
