using UnityEngine;

public class Camera_moving : MonoBehaviour
{
    [Header("Целевой объект (Автожир)")]
    public Transform target;

    [Header("Скорости (float)")]
    public float rotationSpeed = 5f;
    public float moveSpeed = 0.5f;
    public float zoomSpeed = 10f;

    [Header("Пределы перемещения (int)")]
    public int horizontalLimit = 5;
    public int verticalLimit = 3;

    [Header("Ограничения зума")]
    public float minDistance = 3f;
    public float maxDistance = 30f;

    [Header("Плавность фокуса UI")]
    public float focusSmoothSpeed = 5f;

    private float currentDistance;
    private float yaw = 0f;
    private float pitch = 20f;
    private Vector3 panOffset;

    
    private float targetYaw;
    private float targetPitch;
    private float targetDistance;
    private Vector3 targetPanOffset;
    private bool isForcedFocus = false;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Не назначен целевой объект (автожир)! Перетащи автожир в поле Target в инспекторе.");
            return;
        }

        currentDistance = Vector3.Distance(transform.position, target.position);

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        
        targetYaw = yaw;
        targetPitch = pitch;
        targetDistance = currentDistance;
        targetPanOffset = panOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            isForcedFocus = false;
            targetYaw = yaw;
            targetPitch = pitch;
            targetDistance = currentDistance;
            targetPanOffset = panOffset;
        }

        if (!isForcedFocus)
        {
            HandleRotation();
            HandleZoom();
            HandlePanning();
        }

        UpdatePosition();
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            yaw += mouseX;
            pitch -= mouseY;

            pitch = Mathf.Clamp(pitch, -80f, 80f);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
    }

    void HandlePanning()
    {
        if (Input.GetMouseButton(2))
        {
            float moveX = Input.GetAxis("Mouse X") * moveSpeed;
            float moveY = Input.GetAxis("Mouse Y") * moveSpeed;

            Vector3 right = -transform.right;
            Vector3 up = -transform.up;

            right.y = 0;
            right.Normalize();

            panOffset += right * moveX + up * moveY;

            panOffset.x = Mathf.Clamp(panOffset.x, -horizontalLimit, horizontalLimit);
            panOffset.y = Mathf.Clamp(panOffset.y, -verticalLimit, verticalLimit);
        }

        if (Input.GetMouseButtonDown(2) && Input.GetKey(KeyCode.LeftControl))
        {
            panOffset = Vector3.zero;
        }
    }

    void UpdatePosition()
    {
        // Если активирован фокус из меню — плавно приближаем значения к целям
        if (isForcedFocus)
        {
            yaw = Mathf.LerpAngle(yaw, targetYaw, Time.deltaTime * focusSmoothSpeed);
            pitch = Mathf.Lerp(pitch, targetPitch, Time.deltaTime * focusSmoothSpeed);
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * focusSmoothSpeed);
            panOffset = Vector3.Lerp(panOffset, targetPanOffset, Time.deltaTime * focusSmoothSpeed);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * Vector3.back;
        Vector3 targetPos = target.position + panOffset;

        transform.position = targetPos + direction * currentDistance;
        transform.LookAt(targetPos);
    }

    // --- Публичный метод, который будет вызываться из UI кнопок ---
    public void SetCameraFocus(float targetYawAngle, float targetPitchAngle, float distance, Vector3 offset)
    {
        targetYaw = targetYawAngle;
        targetPitch = targetPitchAngle;
        targetDistance = distance;
        targetPanOffset = offset;
        isForcedFocus = true;
    }

    // --- Сброс камеры в дефолтный вид (для кнопки ПРАКТИКА) ---
    public void ResetCameraFocus()
    {
        isForcedFocus = false;
        targetYaw = target.eulerAngles.y + 180f;
        targetPitch = 20f;
        targetDistance = 8f;
        targetPanOffset = Vector3.zero;
        isForcedFocus = true;
    }
}
