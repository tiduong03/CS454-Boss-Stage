using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;

    // Dash state
    private bool isDashing;
    private float dashTimeLeft;
    private float lastDashTime = -999f;

    public bool IsDashing => isDashing;
    public float DashSpeed => dashSpeed;


    public bool CanDash(bool isKnockedBack)
    {
        if (!Input.GetKeyDown(KeyCode.LeftShift) || isKnockedBack || isDashing) return false;
        if (Time.time < lastDashTime + dashCooldown) return false;

        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;

        return true;
    }

    public void UpdateDashTimer()
    {
        if (!isDashing) return;

        dashTimeLeft -= Time.deltaTime;
        if (dashTimeLeft <= 0f)
            isDashing = false;
    }

    public void StopDash()
    {
        isDashing = false;
        dashTimeLeft = 0f;
    }
}
