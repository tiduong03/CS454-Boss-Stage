using UnityEngine;
using System.Collections;
public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 10;

    [Header("Boss")]
    [SerializeField] private bool isBoss = false;

    [Header("Hit Flash")]
    [SerializeField] private bool flashOnHit = true;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private int flashCount = 2;

    [Header("Damage Number")]
    [SerializeField] private FloatingDamageText damageTextPrefab;
    [SerializeField] private Transform damageTextSpawnPoint;
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0f, 1.2f, 0f);

    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Coroutine flashRoutine;

    private void Awake()
    {
        CurrentHP = maxHP;
        IsDead = false;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                originalColors[i] = spriteRenderers[i].color;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead) return;
        if (damageAmount <= 0) return;

        CurrentHP -= damageAmount;
        if (CurrentHP < 0) CurrentHP = 0;

        Debug.Log($"{gameObject.name} took {damageAmount} damage. HP now: {CurrentHP}/{maxHP}");

        ShowDamageNumber(damageAmount);

        if (flashOnHit && spriteRenderers.Length > 0)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(FlashDamageRoutine());
        }

        if (CurrentHP == 0)
            Die();
    }

    private void ShowDamageNumber(int damageAmount)
    {
        if (damageTextPrefab == null)
            return;

        Vector3 spawnPosition = damageTextSpawnPoint != null
            ? damageTextSpawnPoint.position
            : transform.position + damageTextOffset;

        FloatingDamageText popup = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);
        popup.Initialize(damageAmount);
    }

    private IEnumerator FlashDamageRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            SetAllSpriteColors(hitColor);
            yield return new WaitForSeconds(flashDuration);

            RestoreOriginalColors();
            yield return new WaitForSeconds(flashDuration);
        }

        flashRoutine = null;
    }

    private void SetAllSpriteColors(Color color)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = color;
        }
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = originalColors[i];
        }
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