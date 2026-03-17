using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float hitCooldown = 1.5f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 6f;
    [SerializeField] private float knockbackHeight = 3f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private float lastHitTime = -999f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryHitPlayer(collision.transform);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryHitPlayer(other.transform);
    }

    private void TryHitPlayer(Transform contactTarget)
    {
        if (contactTarget == null) return;
        if (!OffCooldown()) return;

        PlayerHealth playerHealth = contactTarget.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) return;

        Transform playerRoot = playerHealth.transform.root;
        if (!playerRoot.CompareTag("Player")) return;

        lastHitTime = Time.time;
        playerHealth.TakeDamage(damage);

        ApplyKnockback(playerRoot);
    }

    private bool OffCooldown()
    {
        return Time.time >= lastHitTime + hitCooldown;
    }

    private void ApplyKnockback(Transform playerRoot)
    {
        if (playerRoot == null) return;

        PlayerKnockbackReceiver knockback = playerRoot.GetComponent<PlayerKnockbackReceiver>();
        if (knockback == null)
            knockback = playerRoot.GetComponentInChildren<PlayerKnockbackReceiver>();

        if (knockback == null) return;

        float direction = (playerRoot.position.x >= transform.position.x) ? 1f : -1f;
        Vector2 velocity = new Vector2(direction * knockbackDistance, knockbackHeight);

        knockback.StartKnockback(velocity, knockbackDuration);
    }
}