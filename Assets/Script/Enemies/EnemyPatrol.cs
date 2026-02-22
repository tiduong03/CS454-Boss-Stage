using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float leftDistance = 2f;
    [SerializeField] private float rightDistance = 2f;
    [SerializeField] private float patrolSpeed = 3f;

    public float PatrolSpeed => patrolSpeed;

    private float spawnPosition;
    private float leftPatrolPosition;
    private float rightPatrolPosition;

    public void SetPatrolBounds(Transform enemy)
    {
        spawnPosition = enemy.position.x;
        leftPatrolPosition = spawnPosition - leftDistance;
        rightPatrolPosition = spawnPosition + rightDistance;
    }

    public bool EnemyOutOfPatrolBounds(Transform enemy)
    {
        float enemyPosition = enemy.position.x;
        return enemyPosition < leftPatrolPosition || enemyPosition > rightPatrolPosition;
    }

    public int GetReturnDirection(Transform enemy)
    {
        float enemyPosition = enemy.position.x;
        if (enemyPosition < leftPatrolPosition) return 1;
        if (enemyPosition > rightPatrolPosition) return -1;
        return 1;
    }

    public void UpdatePatrolDirection(ref int direction, Transform enemy)
    {
        float enemyPosition = enemy.position.x;
        if (direction == 1 && enemyPosition >= rightPatrolPosition) direction = -1;
        if (direction == -1 && enemyPosition <= leftPatrolPosition) direction = 1;
    }

    void OnDrawGizmosSelected()
    {
        float spawnPoint = transform.position.x;
        Vector3 leftPatrolPoint = new(spawnPoint - leftDistance, transform.position.y, 0);
        Vector3 rightPatrolPoint = new(spawnPoint + rightDistance, transform.position.y, 0);

        Gizmos.DrawLine(leftPatrolPoint, rightPatrolPoint);
    }
}