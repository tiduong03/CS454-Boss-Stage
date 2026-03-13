using UnityEngine;

public class PlayerBasicMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Visual Flip")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform visualRoot;

    private float moveInput;
    private int direction = 1; // 1 = right, -1 = left

    public float MoveInput => moveInput;
    public int Direction => direction;
    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (visualRoot == null)
            visualRoot = transform;
    }

    public void ReadMoveInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    public void UpdateFacingDirection()
    {
        if (moveInput > 0)
            direction = 1;
        else if (moveInput < 0)
            direction = -1;

        ApplyVisualFlip();
    }

    private void ApplyVisualFlip()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0;
            return;
        }

        if (visualRoot != null)
        {
            Vector3 scale = visualRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            visualRoot.localScale = scale;
        }
    }
}