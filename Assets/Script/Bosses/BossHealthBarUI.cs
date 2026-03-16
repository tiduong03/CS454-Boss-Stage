using UnityEngine;
using UnityEngine.UI;

public class BossHPBarUI : MonoBehaviour
{
    [SerializeField] private EnemyHealth targetHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private bool hideWhenFull = false;

    private void Awake()
    {
        if (targetHealth == null)
            targetHealth = GetComponentInParent<EnemyHealth>();

        transform.localPosition = localOffset;
    }

    private void OnEnable()
    {
        if (targetHealth == null || fillImage == null)
            return;

        targetHealth.OnHealthChanged += Refresh;
        Refresh(targetHealth.CurrentHP, targetHealth.maxHP);
    }

    private void OnDisable()
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= Refresh;
    }

    private void LateUpdate()
    {
        // Keeps it locked above the boss
        transform.localPosition = localOffset;
    }

    private void Refresh(int currentHP, int maxHP)
    {
        float percent = maxHP <= 0 ? 0f : (float)currentHP / maxHP;
        fillImage.fillAmount = percent;

        if (canvasGroup != null)
        {
            bool visible = currentHP > 0 && (!hideWhenFull || currentHP < maxHP);
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }
}