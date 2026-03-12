using UnityEngine;
using TMPro;

public class PlayerHpUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RectTransform fillRect;   // drag HealthBar RectTransform here (or leave blank)
    [SerializeField] private TMP_Text hpText;          // drag HealthText here

    [SerializeField] private bool smooth = true;
    [SerializeField] private float lerpSpeed = 12f;

    private float shownRatio = 1f;

    void Awake()
    {
        if (fillRect == null) fillRect = (RectTransform)transform;
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    void Update()
    {
        if (playerHealth == null || playerHealth.maxHP <= 0) return;

        float target = (float)playerHealth.CurrentHP / playerHealth.maxHP;

        shownRatio = smooth
            ? Mathf.Lerp(shownRatio, target, Time.deltaTime * lerpSpeed)
            : target;

        // Shrink from LEFT because pivot.x = 0
        fillRect.localScale = new Vector3(shownRatio, 1f, 1f);

        if (hpText != null)
            hpText.text = $"{playerHealth.CurrentHP}/{playerHealth.maxHP}";
    }
}