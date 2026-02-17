using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float hitCooldown = 0.4f;

    [Header("Knockback")]
    [SerializeField] private float knockbackX = 15f;
    [SerializeField] private float knockbackY = 7.5f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private float lastHitTime = -999f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        Transform contactTarget = collision.transform;

        if (!contactTarget.CompareTag("Player")) return;
        if (!CanHit()) return;

        PlayerHealth playerHealth = contactTarget.GetComponentInParent<PlayerHealth>();
        if (!playerHealth) return;

        Hit(playerHealth);
        Knockback(contactTarget);
    }

    bool CanHit()
    {
        return Time.time >= lastHitTime + hitCooldown;
    }

    void Hit(PlayerHealth playerHealth)
    {
        lastHitTime = Time.time;
        playerHealth.TakeDamage(damage); // rename this if your function name differs
    }

    void Knockback(Transform contactTarget)
    {
        // Push player away from the enemy
        float knockbackDirection = (contactTarget.position.x >= transform.position.x) ? 1f : -1f;
        Vector2 knockbackVelocity = new Vector2(knockbackDirection * knockbackX, knockbackY);

        PlayerControl playerControl = contactTarget.GetComponentInParent<PlayerControl>();
        if (playerControl)
        {
            playerControl.ApplyKnockback(knockbackVelocity, knockbackDuration);
            return;
        }
    }
}
