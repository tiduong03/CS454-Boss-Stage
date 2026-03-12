using System.Security.Cryptography;
using UnityEngine;

public class PlayerKnockbackReceiver : MonoBehaviour
{
    private bool isKnockedBack;
    private float knockbackEndTime;
    private Vector2 knockbackVelocity;

    public bool IsKnockedback => isKnockedBack;
    public Vector2 Velocity => knockbackVelocity;

    public void StartKnockback(Vector2 newVelocity, float duration)
    {
        isKnockedBack = true;
        knockbackEndTime = Time.time + duration;
        knockbackVelocity = newVelocity;
    }

    public void UpdateKnockbackStatus()
    {
        if (!isKnockedBack) return;

        if (Time.time >= knockbackEndTime)
            isKnockedBack = false;
    }

    public void StopKnockback()
    {
        isKnockedBack = false;
    }
}
