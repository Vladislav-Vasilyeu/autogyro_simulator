using UnityEngine;

public class JoystickController : MonoBehaviour
{
    [Header("Штурвалы")]
    public Transform joystickLeft;
    public Transform joystickRight;

    [Header("Камера")]
    public CockpitCameraController cameraController;

    [Header("Настройки")]
    public float sensitivity = 2f;

    [Header("Ограничения")]
    public float maxPitch = 20f;

    public float maxRoll = 25f;

    [Header("Возврат")]
    public float returnSpeed = 5f;

    private bool isHolding = false;

    private float currentPitch = 0f;

    private float currentRoll = 0f;

    private Quaternion leftStartRot;
    private Quaternion rightStartRot;

    private void Start()
    {
        leftStartRot = joystickLeft.localRotation;
        rightStartRot = joystickRight.localRotation;
    }

    void Update()
    {
        HandleJoystick();

        UpdateVisuals();
    }

    void HandleJoystick()
    {
        if (Input.GetMouseButtonDown(0)
            && !Input.GetKey(KeyCode.LeftAlt))
        {
            isHolding = true;

            cameraController.isControllingJoystick = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isHolding = false;

            cameraController.isControllingJoystick = false;
        }

        if (isHolding)
        {
            float mouseX = Input.GetAxis("Mouse X");

            float mouseY = Input.GetAxis("Mouse Y");

            currentRoll += mouseX * sensitivity;

            currentPitch -= mouseY * sensitivity;

            currentRoll = Mathf.Clamp(
                currentRoll,
                -maxRoll,
                maxRoll);

            currentPitch = Mathf.Clamp(
                currentPitch,
                -maxPitch,
                maxPitch);
        }
        else
        {
            currentRoll = Mathf.Lerp(
                currentRoll,
                0f,
                Time.deltaTime * returnSpeed);

            currentPitch = Mathf.Lerp(
                currentPitch,
                0f,
                Time.deltaTime * returnSpeed);
        }
    }

    void UpdateVisuals()
    {
        Quaternion offset =
            Quaternion.Euler(
                -currentPitch,
                0f,
                currentRoll);

        joystickLeft.localRotation =
            leftStartRot * offset;

        joystickRight.localRotation =
            rightStartRot * offset;
    }

    public float GetPitchInput()
    {
        return currentPitch / maxPitch;
    }

    public float GetRollInput()
    {
        return currentRoll / maxRoll;
    }
}
