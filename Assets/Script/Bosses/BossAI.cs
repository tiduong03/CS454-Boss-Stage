using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    private enum State { MoveToPoint, Hover, Dash, Vulnerable }

    [Header("Target / Points")]
    [SerializeField] private Transform player;
    [SerializeField] private List<Transform> points = new();
    [SerializeField] private float arriveDistance = 0.15f;

    [Header("Movement")]
    [SerializeField] private float flySpeed = 4f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashTime = 0.35f;

    [Header("Timers")]
    [SerializeField] private float hoverTime = 0.35f;
    [SerializeField] private float vulnerableTime = 0.9f;

    [Header("Layers")]
    [SerializeField] private string enemyLayerName = "Enemy";
    [SerializeField] private string invulnLayerName = "EnemyInvuln";

    [Header("Visual cue (optional)")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color invulnColor = Color.white;
    [SerializeField] private Color vulnerableColor = Color.yellow;

    private Rigidbody2D rb;
    private State state = State.MoveToPoint;
    private Transform currentTarget;
    private float stateEndTime;
    private int dashDir = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        PickNextPoint();
        SetVulnerable(false);
    }

    void Update()
    {
        if (!player || points.Count == 0) return;

        switch (state)
        {
            case State.MoveToPoint:
                MoveTowardPoint();
                break;

            case State.Hover:
                if (Time.time >= stateEndTime) StartDash();
                break;

            case State.Dash:
                if (Time.time >= stateEndTime) StartVulnerable();
                break;

            case State.Vulnerable:
                if (Time.time >= stateEndTime) StartMoveToNextPoint();
                break;
        }
    }

    private void MoveTowardPoint()
    {
        if (!currentTarget) { PickNextPoint(); return; }

        Vector2 pos = rb.position;
        Vector2 target = currentTarget.position;
        Vector2 next = Vector2.MoveTowards(pos, target, flySpeed * Time.deltaTime);
        rb.MovePosition(next);

        if (Vector2.Distance(next, target) <= arriveDistance)
            StartHover();
    }

    private void StartHover()
    {
        state = State.Hover;
        stateEndTime = Time.time + hoverTime;
        rb.linearVelocity = Vector2.zero;
        SetVulnerable(false);
    }

    private void StartDash()
    {
        state = State.Dash;
        stateEndTime = Time.time + dashTime;

        // dash horizontally toward the player's side
        dashDir = (player.position.x >= transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        SetVulnerable(false);
    }

    private void StartVulnerable()
    {
        state = State.Vulnerable;
        stateEndTime = Time.time + vulnerableTime;
        rb.linearVelocity = Vector2.zero;

        SetVulnerable(true);
    }

    private void StartMoveToNextPoint()
    {
        SetVulnerable(false);
        PickNextPoint();
        state = State.MoveToPoint;
    }

    private void PickNextPoint()
    {
        // simple: pick a random point different from current
        if (points.Count == 0) return;

        Transform next = currentTarget;
        int safety = 20;
        while (next == currentTarget && safety-- > 0)
            next = points[Random.Range(0, points.Count)];

        currentTarget = next;
    }

    private void SetVulnerable(bool yes)
    {
        int layer = LayerMask.NameToLayer(yes ? enemyLayerName : invulnLayerName);
        if (layer != -1)
        {
            gameObject.layer = layer;
            // apply to children too (so PlayerAttack mask works reliably)
            foreach (Transform t in GetComponentsInChildren<Transform>())
                t.gameObject.layer = layer;
        }

        if (sr) sr.color = yes ? vulnerableColor : invulnColor;
    }
}