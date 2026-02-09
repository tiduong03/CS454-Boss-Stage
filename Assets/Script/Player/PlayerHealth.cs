using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 100;

    public int CurrentHP { get; private set; }  // other scripts can read, not set directly
    public bool IsDead { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        CurrentHP = maxHP;
        IsDead = false;

        Debug.Log("Player HP: " + CurrentHP);
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead) return;
        if (damageAmount <= 0) return;

        CurrentHP -= damageAmount;
        if (CurrentHP < 0) CurrentHP = 0;

        Debug.Log($"Player took {damageAmount} damage. HP now: {CurrentHP}/{maxHP}");

        if (CurrentHP == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        CurrentHP += amount;
        if (CurrentHP > maxHP) CurrentHP = maxHP;

        Debug.Log($"Player healed {amount}. HP now: {CurrentHP}/{maxHP}");
    }

    void Die()
    {
        IsDead = true;
        Debug.Log("Player died!");

        // disable movement script so player can't move after death
        var controller = GetComponent<PlayerController2D>();
        if (controller != null) controller.enabled = false;

        // (later) show lose screen / restart level
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //Testing
    void Update()
    {
        // Press H to take 10 damage (testing)
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10);
        }

        // Press J to heal 10 (testing)
        if (Input.GetKeyDown(KeyCode.J))
        {
            Heal(10);
        }
    }
}
