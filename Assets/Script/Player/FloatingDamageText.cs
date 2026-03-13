using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private TMP_Text textUI;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float lifeTime = 0.6f;
    [SerializeField] private Vector3 randomOffset = new Vector3(0.25f, 0.1f, 0f);

    private Color startColor;
    private float timer;

    void Awake()
    {
        if (textUI == null)
            textUI = GetComponentInChildren<TMP_Text>();

        if (textUI != null)
            startColor = textUI.color;
    }

    public void Initialize(int damageAmount)
    {
        if (textUI == null)
            textUI = GetComponentInChildren<TMP_Text>();

        if (textUI != null)
        {
            textUI.text = damageAmount.ToString();
            startColor = textUI.color;
        }

        Vector3 offset = new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(0f, randomOffset.y),
            0f
        );

        transform.position += offset;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        timer += dt;

        transform.position += Vector3.up * moveSpeed * dt;

        if (textUI != null)
        {
            float t = Mathf.Clamp01(timer / lifeTime);
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            textUI.color = c;
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }
}