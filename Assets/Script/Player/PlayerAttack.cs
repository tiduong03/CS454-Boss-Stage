using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private float attackCooldown = 0.25f;

    [Header("Hitbox")]
    [SerializeField] private Vector2 attackOffset = new Vector2(0.85f, 0f);

    [Header("Enemy Filter")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 12f;
    [SerializeField] private float knockbackHeight = 6f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private int attackDirection;
    private float lastAttackTime = -999f;

    public void TryAttack(int facingDirection, bool isKnockedBack, bool isDashing)
    {
        attackDirection = facingDirection;

        if (!Input.GetMouseButtonDown(0)) return;
        if (isKnockedBack || isDashing) return;
        if (OnCooldown()) return;

        lastAttackTime = Time.time;
        Attack(facingDirection);
    }

    private bool OnCooldown()
    {
        return Time.time < lastAttackTime + attackCooldown;
    }

    private void Attack(int facingDir)
    {
        Vector2 attackHitBox = GetAttackHitBox(facingDir);
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackHitBox, attackRange, enemyLayer);

        foreach (Collider2D enemy in enemiesHit)
        {
            if (!enemy) continue;

            DealDamage(enemy);
            ApplyKnockback(enemy);
        }
    }

    private Vector2 GetAttackHitBox(int facingDir)
    {
        return (Vector2)transform.position + new Vector2(facingDir * attackOffset.x, attackOffset.y);
    }

    private void DealDamage(Collider2D enemy)
    {
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health)
            health.TakeDamage(damage);
    }

    private void ApplyKnockback(Collider2D enemy)
    {
        EnemyKnockbackReceiver knockback = enemy.GetComponent<EnemyKnockbackReceiver>();
        if (!knockback) return;

        float direction = (enemy.transform.position.x >= transform.position.x) ? 1f : -1f;
        Vector2 velocity = new Vector2(direction * knockbackDistance, knockbackHeight);

        knockback.StartKnockback(velocity, knockbackDuration);
    }

    void OnDrawGizmosSelected()
    {
        // Shows the attack range in the Scene view
        Vector2 center = GetAttackHitBox(attackDirection);

        Gizmos.DrawWireSphere(center, attackRange);
    }
}
