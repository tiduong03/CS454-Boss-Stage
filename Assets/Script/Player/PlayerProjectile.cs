using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D projectileCollider;

    private Transform owner;
    private int damage;
    private float knockbackDistance;
    private float knockbackHeight;
    private float knockbackDuration;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        projectileCollider.isTrigger = true;
    }

    public void Initialize(Transform ownerTransform, Vector2 direction, float speed, int projectileDamage, float lifetime, float enemyKnockbackDistance, float enemyKnockbackHeight, float enemyKnockbackDuration)
    {
        owner = ownerTransform;
        damage = projectileDamage;
        knockbackDistance = enemyKnockbackDistance;
        knockbackHeight = enemyKnockbackHeight;
        knockbackDuration = enemyKnockbackDuration;

        Vector2 moveDirection = direction.sqrMagnitude <= 0.001f ? Vector2.right : direction.normalized;
        rb.linearVelocity = moveDirection * speed;

        // Flip projectile sprite when shooting left
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (moveDirection.x >= 0f ? 1f : -1f);
        transform.localScale = scale;

        IgnoreOwnerCollisions();
        Destroy(gameObject, lifetime);
    }

    private void IgnoreOwnerCollisions()
    {
        if (!owner || !projectileCollider) return;

        Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D ownerCol in ownerColliders)
        {
            if (ownerCol)
                Physics2D.IgnoreCollision(projectileCollider, ownerCol, true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other) return;
        if (owner && other.transform.IsChildOf(owner)) return;

        EnemyHealth health = other.GetComponentInParent<EnemyHealth>();
        if (health)
        {
            health.TakeDamage(damage);
            ApplyKnockback(other);
            Destroy(gameObject);
            return;
        }

        // Destroy on walls / ground / other solid colliders
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyKnockback(Collider2D hit)
    {
        EnemyKnockbackReceiver knockback = hit.GetComponentInParent<EnemyKnockbackReceiver>();
        if (!knockback) return;

        float direction = rb.linearVelocity.x >= 0f ? 1f : -1f;
        Vector2 velocity = new Vector2(direction * knockbackDistance, knockbackHeight);

        knockback.StartKnockback(velocity, knockbackDuration);
    }
}