using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTriggerName = "Attack";

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 12f;
    [SerializeField] private float knockbackHeight = 6f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Range Attack")]
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private int projectileLifetime = 3;

    private int attackDirection;
    private float lastAttackTime = -999f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void TryAttack(int facingDirection, bool isKnockedBack, bool isDashing)
    {
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.H) && !Input.GetButtonDown("Fire1")) return;
        if (isKnockedBack || isDashing) return;
        if (OnCooldown()) return;

        lastAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger(attackTriggerName);

        FireShot(facingDirection);
    }

    private bool OnCooldown()
    {
        return Time.time < lastAttackTime + attackCooldown;
    }

    private void FireShot(int facingDirection)
    {
        if (!projectilePrefab)
        {
            Debug.LogWarning($"{name}: Assign a PlayerProjectile prefab.");
            return;
        }

        Transform origin = firePoint ? firePoint : transform;
        Vector2 direction = facingDirection >= 0 ? Vector2.right : Vector2.left;

        PlayerProjectile projectile = Instantiate(projectilePrefab, origin.position, Quaternion.identity);
        projectile.Initialize(transform, direction, projectileSpeed, projectileDamage, projectileLifetime,
            knockbackDistance, knockbackHeight, knockbackDuration);
    }
}