using UnityEngine;

public class CockpitCameraController : MonoBehaviour
{
    [Header("Мышь")]
    public float mouseSensitivity = 2f;

    [Header("Ограничения обзора")]
    public float minPitch = -60f;
    public float maxPitch = 60f;

    [Header("Zoom")]
    public Camera cockpitCamera;

    public float normalFOV = 60f;
    public float zoomFOV = 25f;

    public float zoomSpeed = 8f;

    private float yaw = 0f;
    private float pitch = 0f;

    [HideInInspector]
    public bool isControllingJoystick = false;

    void Start()
    {
        Vector3 angles = transform.localEulerAngles;

        yaw = angles.y;
        pitch = angles.x;

        cockpitCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        HandleCursorMode();

        if (!Input.GetKey(KeyCode.LeftAlt) && !isControllingJoystick)
        {
            RotateCamera();
        }

        HandleZoom();
    }

    void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleZoom()
    {
        float targetFOV = Input.GetMouseButton(1)
            ? zoomFOV
            : normalFOV;

        cockpitCamera.fieldOfView = Mathf.Lerp(
            cockpitCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * zoomSpeed
        );
    }

    void HandleCursorMode()
    {
        bool freeCursor = Input.GetKey(KeyCode.LeftAlt);

        Cursor.visible = freeCursor;

        Cursor.lockState = freeCursor
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }
}
