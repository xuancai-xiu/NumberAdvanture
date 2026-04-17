using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float cameraSize = 5f;
    [SerializeField] private Vector3 fixedPosition = new Vector3(0, 0, -10f);
    [SerializeField] private bool followPlayer = false;
    [SerializeField] private float offsetY = 2f;

    private Camera mainCamera;
    private Player player;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        player = FindObjectOfType<Player>();

        // Set camera size for fixed orthographic view
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = cameraSize;
            
            if (!followPlayer)
            {
                // Fixed camera position
                transform.position = fixedPosition;
            }
        }
    }

    private void LateUpdate()
    {
        if (followPlayer && player != null && mainCamera != null)
        {
            // Keep camera centered on player with slight upward offset
            Vector3 newPos = player.transform.position + Vector3.up * offsetY;
            newPos.z = -10f;
            transform.position = newPos;
        }
    }
}
