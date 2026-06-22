using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Камеры")]
    public Camera thirdPersonCamera;
    public Camera cockpitCamera;

    private bool isCockpitView = false;

    void Start()
    {
        SetThirdPersonView();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
        }
    }

    void SwitchCamera()
    {
        isCockpitView = !isCockpitView;

        if (isCockpitView)
        {
            SetCockpitView();
        }
        else
        {
            SetThirdPersonView();
        }
    }

    void SetCockpitView()
    {
        cockpitCamera.gameObject.SetActive(true);
        thirdPersonCamera.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SetThirdPersonView()
    {
        cockpitCamera.gameObject.SetActive(false);
        thirdPersonCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
