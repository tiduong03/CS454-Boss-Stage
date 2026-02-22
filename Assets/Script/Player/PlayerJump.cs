using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // Jump state
    private bool isGrounded;

    public float JumpForce => jumpForce;

     
    public void UpdateGroundedState()
    {
        if (!groundCheck) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public bool CanJump(bool isKnockedBack, bool isDashing)
    {
        //Ground check
        UpdateGroundedState();

        if (!Input.GetButtonDown("Jump") || !isGrounded || isKnockedBack || isDashing) return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        // Draw groundcheck
        if (!groundCheck) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}