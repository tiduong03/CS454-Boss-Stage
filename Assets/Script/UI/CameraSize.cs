using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BossRoomCameraFit : MonoBehaviour
{
    [SerializeField] private BoxCollider2D ground;
    [SerializeField] private BoxCollider2D ceiling;
    [SerializeField] private BoxCollider2D wallLeft;
    [SerializeField] private BoxCollider2D wallRight;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    private void Start()
    {
        float left = wallLeft.bounds.min.x;
        float right = wallRight.bounds.max.x;
        float bottom = ground.bounds.min.y;
        float top = ceiling.bounds.max.y;

        float width = right - left;
        float height = top - bottom;

        float centerX = (left + right) * 0.5f;
        float centerY = (top + bottom) * 0.5f;

        transform.position = new Vector3(centerX, centerY, -10f);

        float sizeByHeight = height * 0.5f;
        float sizeByWidth = width / (2f * cam.aspect);

        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
    }
}
