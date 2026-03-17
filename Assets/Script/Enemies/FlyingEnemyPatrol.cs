using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyPatrol : MonoBehaviour
{
    [Header("Find Platforms Automatically")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private string platformTag = "Stage";

    [Header("Arena Patrol")]
    [SerializeField] private float hoverHeightAbovePlatform = 2f;
    [SerializeField] private float platformEdgePadding = 0.5f;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float retargetTime = 2f;
    [SerializeField] private float arriveDistance = 0.2f;
    [SerializeField] private float hoverAmplitude = 0.25f;
    [SerializeField] private float hoverFrequency = 2f;
    [SerializeField] private float searchRadius = 50f;

    private readonly List<Collider2D> arenaPlatforms = new();

    private Vector2 moveTarget;
    private float retargetTimer;

    public float PatrolSpeed => patrolSpeed;

    private void Start()
    {
        FindPlatformsInScene();
        PickNewMoveTarget();
        retargetTimer = retargetTime;
    }

    private void Update()
    {
        retargetTimer -= Time.deltaTime;
    }

    public Vector2 GetPatrolDirection(Transform enemy)
    {
        if (enemy == null) return Vector2.zero;

        bool reachedTarget = Vector2.Distance(enemy.position, moveTarget) <= arriveDistance;

        if (reachedTarget || retargetTimer <= 0f)
        {
            PickNewMoveTarget();
            retargetTimer = retargetTime;
        }

        Vector2 hoverOffset = new Vector2(
            0f,
            Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude
        );

        Vector2 desiredPosition = moveTarget + hoverOffset;
        Vector2 direction = desiredPosition - (Vector2)enemy.position;

        if (direction.magnitude <= 0.05f)
            return Vector2.zero;

        return direction.normalized;
    }

    private void FindPlatformsInScene()
    {
        arenaPlatforms.Clear();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius, platformLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null) continue;

            if (!string.IsNullOrEmpty(platformTag) && !hit.CompareTag(platformTag))
                continue;

            arenaPlatforms.Add(hit);
        }

        if (arenaPlatforms.Count == 0)
        {
            Debug.LogWarning($"{name}: No patrol platforms found. Check platform layer/tag settings.");
        }
    }

    private void PickNewMoveTarget()
    {
        if (arenaPlatforms.Count == 0)
        {
            moveTarget = transform.position;
            return;
        }

        Collider2D platform = GetRandomValidPlatform();
        if (platform == null)
        {
            moveTarget = transform.position;
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

    private Collider2D GetRandomValidPlatform()
    {
        List<Collider2D> validPlatforms = new();

        for (int i = 0; i < arenaPlatforms.Count; i++)
        {
            if (arenaPlatforms[i] != null)
                validPlatforms.Add(arenaPlatforms[i]);
        }

        if (validPlatforms.Count == 0)
            return null;

        return validPlatforms[Random.Range(0, validPlatforms.Count)];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
        Gizmos.DrawWireSphere(moveTarget, 0.2f);
    }
}