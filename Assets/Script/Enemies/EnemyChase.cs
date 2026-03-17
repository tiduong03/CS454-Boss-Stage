using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Chase")]
    [SerializeField] private float chaseRange = 6f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float stopDistance = 0.8f;

    public float ChaseSpeed => chaseSpeed;

    public void SetTargetToChase(ref Transform player)
    {
        if (player != null) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    public bool PlayerInChaseRange(Transform enemy, Transform player)
    {
        if (enemy == null || player == null) return false;

        float dist = Vector2.Distance(enemy.position, player.position);
        return dist <= chaseRange;
    }

    public Vector2 GetChaseDirection(Transform enemy, Transform player)
    {
        if (enemy == null || player == null) return Vector2.zero;

        Vector2 offset = player.position - enemy.position;
        float distance = offset.magnitude;

        if (distance <= stopDistance)
            return Vector2.zero;

        return offset.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}