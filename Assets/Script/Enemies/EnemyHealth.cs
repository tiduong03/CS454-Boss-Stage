using UnityEngine;
using System;
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

    public event Action<int, int> OnHealthChanged;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Coroutine flashRoutine;

    private static int activeBossCount = 0;
    private bool bossRegistered = false;
    public static event Action OnAllBossesDefeated;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]

    private static void ResetBossCounter()
    {
        activeBossCount = 0;
    }

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

        NotifyHealthChanged();
    }

    private void OnEnable()
    {
        RegisterBoss();
    }

    private void OnDisable()
    {
        if (!IsDead)
            UnregisterBoss(false);
    }

    private void RegisterBoss()
    {
        if (!isBoss || bossRegistered)
            return;

        activeBossCount++;
        bossRegistered = true;
    }

    private void UnregisterBoss(bool checkForWin)
    {
        if (!isBoss || !bossRegistered)
            return;

        bossRegistered = false;
        activeBossCount = Mathf.Max(0, activeBossCount - 1);

        if (checkForWin && activeBossCount == 0)
        {
            if (GameUIManager.Instance != null)
                GameUIManager.Instance.ShowWin();

            OnAllBossesDefeated?.Invoke();
        }
    }

    public void SetCurrentHP(int value)
    {
        CurrentHP = Mathf.Clamp(value, 0, maxHP);
        NotifyHealthChanged();

        if (CurrentHP == 0 && !IsDead)
            Die();
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead) return;
        if (damageAmount <= 0) return;

        CurrentHP -= damageAmount;
        if (CurrentHP < 0) CurrentHP = 0;

        Debug.Log($"{gameObject.name} took {damageAmount} damage. HP now: {CurrentHP}/{maxHP}");

        NotifyHealthChanged();
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

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
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

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        RestoreOriginalColors();

        if (isBoss)
            UnregisterBoss(true);

        gameObject.SetActive(false);
    }
}