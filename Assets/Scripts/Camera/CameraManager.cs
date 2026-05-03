using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float cameraSize = 5f;
    [SerializeField] private Vector3 fixedPosition = new Vector3(0, 0, -10f);
    [SerializeField] private bool followPlayer = false;
    [SerializeField] private float offsetY = 2f;
    [SerializeField] private float targetAspectWidth = 16f;
    [SerializeField] private float targetAspectHeight = 9f;

    [Header("Window Settings")]
    [SerializeField] private int targetScreenWidth = 1920;
    [SerializeField] private int targetScreenHeight = 1080;
    [SerializeField] private bool useFullScreenWindow = true;

    private Camera mainCamera;
    private Player player;
    private float targetAspect;         

    private int lastScreenWidth;
    private int lastScreenHeight;
    private float lastTargetAspect;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        player = FindObjectOfType<Player>();
        targetAspect = targetAspectWidth / targetAspectHeight;
    }

    private void Start()
    {
        if (useFullScreenWindow)
        {
            Screen.SetResolution(targetScreenWidth, targetScreenHeight, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(targetScreenWidth, targetScreenHeight, FullScreenMode.Windowed);
        }

        mainCamera.orthographicSize = cameraSize;

        SetCameraPosition();

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastTargetAspect = targetAspect;
        ApplyAspectRatio();  
    }

    private void LateUpdate()
    {
        if (followPlayer && player != null)
        {
            Vector3 newPos = player.transform.position + Vector3.up * offsetY;
            newPos.z = fixedPosition.z;
            transform.position = newPos;
        }
        else
        {
            SetCameraPosition();
        }

        bool resolutionChanged = (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight);
        bool targetAspectChanged = !Mathf.Approximately(targetAspect, lastTargetAspect);

        if (resolutionChanged || targetAspectChanged)
        {
            if (targetAspectChanged)
            {
                targetAspect = targetAspectWidth / targetAspectHeight;
                lastTargetAspect = targetAspect;
            }

            ApplyAspectRatio();

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    private void SetCameraPosition()
    {
        transform.position = fixedPosition;
    }

    private void ApplyAspectRatio()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            Rect rect = new Rect(0f, (1f - scaleHeight) / 2f, 1f, scaleHeight);
            mainCamera.rect = rect;
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            Rect rect = new Rect((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
            mainCamera.rect = rect;
        }
    }
}