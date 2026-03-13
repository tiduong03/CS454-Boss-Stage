using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private bool destroyOnHit = true;

    private Vector2 direction;
    private float speed;
    private int damage;
    private bool initialized;

    public void Initialize(Vector2 moveDirection, float moveSpeed, int damageAmount)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        damage = damageAmount;
        initialized = true;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!initialized)
            return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized)
            return;

        if (((1 << other.gameObject.layer) & hitLayers) == 0)
            return;

        other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        if (destroyOnHit)
            Destroy(gameObject);
    }
}