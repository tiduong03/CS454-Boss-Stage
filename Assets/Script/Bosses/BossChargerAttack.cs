using UnityEngine;

public class BossChargerAttack : MonoBehaviour
{
    private enum State { Idle, WindupDash, Dashing, WindupSlam, Recover }

    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private float engageRange = 12f;

    [Header("Idle movement")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Dash Attack")]
    [SerializeField] private float dashTriggerRange = 7f;
    [SerializeField] private float dashWindup = 0.35f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.4f;

    [Header("Slam Attack")]
    [SerializeField] private float slamTriggerRange = 5f;
    [SerializeField] private float slamWindup = 0.45f;
    [SerializeField] private float slamRadius = 2.5f;
    [SerializeField] private Vector2 slamOffset = new Vector2(0f, -0.25f);

    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask playerLayer; // set this to Player layer
    [SerializeField] private float hitLockout = 0.25f; // prevents multi-hit spam

    [Header("Knockback (optional)")]
    [SerializeField] private float knockbackDistance = 10f;
    [SerializeField] private float knockbackHeight = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Recovery")]
    [SerializeField] private float recoverTime = 0.7f;
    [SerializeField] private float minTimeBetweenAttacks = 0.8f;

    [Header("Disable these while boss is attacking (optional)")]
    [SerializeField] private Behaviour[] disableWhileAttacking;

    private Rigidbody2D rb;
    private State state = State.Idle;
    private float stateEndTime;
    private float nextAllowedHitTime;
    private float nextAllowedAttackTime;
    private int dir = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (autoFindPlayer && player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (!player && autoFindPlayer)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!player || !rb) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > engageRange)
        {
            // Boss is "sleeping" if you're far away
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        dir = (player.position.x >= transform.position.x) ? 1 : -1;

        switch (state)
        {
            case State.Idle:
                IdleMove(dist);
                TryStartAttack(dist);
                break;

            case State.WindupDash:
                if (Time.time >= stateEndTime) StartDash();
                break;

            case State.Dashing:
                if (Time.time >= stateEndTime) StartRecover();
                break;

            case State.WindupSlam:
                if (Time.time >= stateEndTime) DoSlam();
                break;

            case State.Recover:
                if (Time.time >= stateEndTime) EndRecover();
                break;
        }
    }

    private void IdleMove(float dist)
    {
        // Simple horizontal chase
        float vx = dir * moveSpeed;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    private void TryStartAttack(float dist)
    {
        if (Time.time < nextAllowedAttackTime) return;

        // Prefer slam when very close, dash when mid-range
        if (dist <= slamTriggerRange)
        {
            StartSlamWindup();
            return;
        }

        if (dist <= dashTriggerRange)
        {
            StartDashWindup();
            return;
        }
    }

    private void StartDashWindup()
    {
        SetDisabled(true);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        state = State.WindupDash;
        stateEndTime = Time.time + dashWindup;
    }

    private void StartDash()
    {
        state = State.Dashing;
        stateEndTime = Time.time + dashDuration;

        rb.linearVelocity = new Vector2(dir * dashSpeed, rb.linearVelocity.y);
    }

    private void StartSlamWindup()
    {
        SetDisabled(true);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        state = State.WindupSlam;
        stateEndTime = Time.time + slamWindup;
    }

    private void DoSlam()
    {
        // Slam hits in a circle
        Vector2 center = (Vector2)transform.position + slamOffset;
        Collider2D hit = Physics2D.OverlapCircle(center, slamRadius, playerLayer);

        if (hit && hit.CompareTag("Player"))
        {
            Debug.Log("Slam atk");
            DealDamage(hit.transform);
            ApplyKnockback(hit.transform, dir);
        }

        StartRecover();
    }

    private void StartRecover()
    {
        // Stop after attack
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        state = State.Recover;
        stateEndTime = Time.time + recoverTime;
        nextAllowedAttackTime = Time.time + minTimeBetweenAttacks;
    }

    private void EndRecover()
    {
        state = State.Idle;
        SetDisabled(false);
    }

    private void DealDamage(Transform target)
    {
        if (Time.time < nextAllowedHitTime) return;
        nextAllowedHitTime = Time.time + hitLockout;

        var ph = target.GetComponentInParent<PlayerHealth>();
        if (ph) ph.TakeDamage(damage);
    }

    private void ApplyKnockback(Transform target, int hitDir)
    {
        var kb = target.GetComponentInParent<PlayerKnockbackReceiver>();
        if (!kb) return;

        Vector2 vel = new Vector2(hitDir * knockbackDistance, knockbackHeight);
        kb.StartKnockback(vel, knockbackDuration);
    }

    private void SetDisabled(bool disabled)
    {
        if (disableWhileAttacking == null) return;
        for (int i = 0; i < disableWhileAttacking.Length; i++)
        {
            if (disableWhileAttacking[i] != null)
                disableWhileAttacking[i].enabled = !disabled;
        }
    }

    // Dash damage: only hurts if boss collides WITH player during Dashing
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (state != State.Dashing) return;
        if (!col.collider.CompareTag("Player")) return;

        DealDamage(col.collider.transform);
        ApplyKnockback(col.collider.transform, dir);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state != State.Dashing) return;
        if (!other.CompareTag("Player")) return;

        DealDamage(other.transform);
        ApplyKnockback(other.transform, dir);
    }

    private void OnDrawGizmosSelected()
    {
        // Slam preview
        Vector2 center = (Vector2)transform.position + slamOffset;
        Gizmos.DrawWireSphere(center, slamRadius);

        // Engage + trigger previews
        Gizmos.DrawWireSphere(transform.position, engageRange);
        Gizmos.DrawWireSphere(transform.position, dashTriggerRange);
        Gizmos.DrawWireSphere(transform.position, slamTriggerRange);
    }
}