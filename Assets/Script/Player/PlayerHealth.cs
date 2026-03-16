using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 100;

    [Header("Hit Flash")]
    [SerializeField] private bool flashOnHit = true;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private int flashCount = 2;

    [Header("Damage Number")]
    [SerializeField] private FloatingDamageText damageTextPrefab;
    [SerializeField] private Transform damageTextSpawnPoint;
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0f, 1.2f, 0f);

    [SerializeField] private PlayerDash dash;
    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Coroutine flashRoutine;

    void Awake()
    {
        //if (!dash) dash = GetComponent<PlayerDash>();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                originalColors[i] = spriteRenderers[i].color;
        }
    }

    void Start()
    {
        CurrentHP = maxHP;
        IsDead = false;
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead) return;
        if (damageAmount <= 0) return;
        if (dash != null && dash.IsDashing) return;

        CurrentHP -= damageAmount;
        if (CurrentHP < 0) CurrentHP = 0;

        Debug.Log($"Player took {damageAmount} damage. HP now: {CurrentHP}/{maxHP}");

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

    void Die()
    {
        IsDead = true;
        Debug.Log("Player died!");

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        RestoreOriginalColors();

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.ShowLose();

        gameObject.SetActive(false);
    }
}