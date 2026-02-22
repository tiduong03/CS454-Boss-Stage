using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float hitCooldown = 0.4f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 6f;
    [SerializeField] private float knockbackHeight = 3f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private float lastHitTime = -999f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        Transform contactTarget = collision.rigidbody ? collision.rigidbody.transform : collision.transform;

        if (!contactTarget.CompareTag("Player")) return;
        if (!OnCooldown()) return;

        bool hitLanded = Hit(contactTarget);
        if (hitLanded) ApplyKnockback(contactTarget);
    }

    bool OnCooldown()
    {
        return Time.time >= lastHitTime + hitCooldown;
    }

    bool Hit(Transform contactTarget)
    {
        PlayerHealth playerHealth = contactTarget.GetComponentInParent<PlayerHealth>();
        if (!playerHealth) return false;

        lastHitTime = Time.time;
        playerHealth.TakeDamage(damage);

        return true;
    }

    void ApplyKnockback(Transform contactTarget)
    {
        PlayerKnockbackReceiver knockback = contactTarget.GetComponentInParent<PlayerKnockbackReceiver>();
        if (!knockback) return;
        
        float direction = (contactTarget.position.x >= transform.position.x) ? 1f : -1f;
        Vector2 velocity = new Vector2(direction * knockbackDistance, knockbackHeight);

        knockback.StartKnockback(velocity, knockbackDuration);
    }
}