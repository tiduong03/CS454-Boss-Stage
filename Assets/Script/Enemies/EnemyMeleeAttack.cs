using Unity.Burst.CompilerServices;
using UnityEngine;

public class EnemyMeleeDamage : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackRadius = 0.8f;
    [SerializeField] private float attackCooldown = 0.8f;

    [Header("Hitbox (PlayerAttack style)")]
    [SerializeField] private Vector2 attackOffset = new Vector2(0.85f, 0f);
    [SerializeField] private LayerMask playerLayer;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 6f;
    [SerializeField] private float knockbackHeight = 3f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private Transform player;
    private float nextAttackTime = 0f;
    private int attackDirection = 1;

    void Awake()
    {
        FindPlayer();
    }

    void Update()
    {
        if (!player) FindPlayer();
        if (!player) return;
        if (Time.time < nextAttackTime) return;

        attackDirection = (player.position.x >= transform.position.x) ? 1 : -1;
        Vector2 attackHitBox = GetAttackHitBox(attackDirection);
        Collider2D playerHit = Physics2D.OverlapCircle(attackHitBox, attackRadius, playerLayer);
        if (!playerHit) return;

        DealDamage(playerHit);
        ApplyKnockback(playerHit);
    }

    private Vector2 GetAttackHitBox(int dir)
    {
        return (Vector2)transform.position + new Vector2(dir * attackOffset.x, attackOffset.y);
    }

    private void FindPlayer()
    {
        if (player) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject) player = playerObject.transform;
    }

    private void DealDamage(Collider2D player)
    {
        PlayerHealth health = player.GetComponentInParent<PlayerHealth>();
        if (health)
            health.TakeDamage(damage);
    }

    private void ApplyKnockback(Collider2D player)
    {
        PlayerKnockbackReceiver knockback = player.GetComponentInParent<PlayerKnockbackReceiver>();
        if (!knockback) return;

        Vector2 vel = new Vector2(attackDirection * knockbackDistance, knockbackHeight);
        knockback.StartKnockback(vel, knockbackDuration);

        nextAttackTime = Time.time + attackCooldown;
    }
    void OnDrawGizmosSelected()
    {
        Vector2 center = GetAttackHitBox(attackDirection);
        Gizmos.DrawWireSphere(center, attackRadius);
    }
}