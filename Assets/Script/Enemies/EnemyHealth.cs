using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 3;

    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        CurrentHP = maxHP;
        IsDead = false;

        //Debug.Log($"{gameObject.name} HP: {CurrentHP}");
    }

    // (later) your player attack / hitbox will call this
    public void TakeDamage(int damageAmount)
    {
        if (IsDead) return;
        if (damageAmount <= 0) return;

        CurrentHP -= damageAmount;
        if (CurrentHP < 0) CurrentHP = 0;

        Debug.Log($"{gameObject.name} took {damageAmount} damage. HP now: {CurrentHP}/{maxHP}");

        if (CurrentHP == 0)
            Die();
    }

    void Die()
    {
        IsDead = true;

        // (later) play death animation
        Destroy(gameObject);
    }
}
