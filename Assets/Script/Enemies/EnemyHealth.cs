using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 10;

    [Header("Boss")]
    [SerializeField] private bool isBoss = false;

    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    private void Awake()
    {
        CurrentHP = maxHP;
        IsDead = false;
    }

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

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // Only show win screen if this enemy is the boss
        if (isBoss && EndScreenUI.Instance != null)
            EndScreenUI.Instance.ShowWin();
        
        gameObject.SetActive(false);
    }
}