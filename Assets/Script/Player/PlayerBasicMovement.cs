using UnityEngine;

public class PlayerBasicMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    // Input + dsirection
    private float moveInput;
    private int direction = 1; // 1 = right, -1 = left

    public float MoveInput => moveInput;
    public int Direction => direction;
    public float MoveSpeed => moveSpeed;

    public void ReadMoveInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    public void UpdateFacingDirection()
    {
        if (moveInput > 0) direction = 1;
        else if (moveInput < 0) direction = -1;
    }
}