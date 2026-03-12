using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Chase")]
    [SerializeField] private float chaseRange = 6f;
    [SerializeField] private float chaseSpeed = 4f;

    public float ChaseSpeed => chaseSpeed;

    public void SetTargetToChase(ref Transform player)
    {
        if (player) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject) player = playerObject.transform;
    }

    public bool PlayerInChaseRange(Transform enemy, Transform player)
    {
        float dist = Vector2.Distance(enemy.position, player.position);
        return dist <= chaseRange;
    }

    public int GetChaseDirection(Transform enemy, Transform player)
    {
        return (player.position.x > enemy.position.x) ? 1 : -1;
    }
}