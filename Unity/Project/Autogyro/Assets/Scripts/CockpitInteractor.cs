using Assets.Scripts;
using TMPro;
using UnityEngine;

public class CockpitInteractor : MonoBehaviour
{
    [Header("Настройки луча")]
    public Camera cockpitCamera;
    public float interactDistance = 2f;

    [Header("UI Подсказки (Tooltip)")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    void Update()
    {
        HandleTooltip();

        if (Input.GetMouseButtonDown(0))
        {
            Interact();
        }
    }

    void Interact()
    {
        Ray ray = cockpitCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            Debug.Log("Нажат объект: " + hit.collider.name);
            IToggleable toggleable = hit.collider.GetComponent<IToggleable>();
            if (toggleable != null)
            {
                toggleable.Toggle();
            }
        }
    }

    void HandleTooltip()
    {
        if (tooltipPanel == null || tooltipText == null || cockpitCamera == null) return;

        Ray ray = cockpitCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            IToggleable toggleable = hit.collider.GetComponent<IToggleable>();
            if (toggleable != null && Input.GetKey(KeyCode.LeftAlt))
            {
                tooltipPanel.SetActive(true);
                tooltipText.text = toggleable.GetDescription();

                Vector3 mousePos = Input.mousePosition;
                tooltipPanel.transform.position = mousePos + new Vector3(15f, -15f, 0f);
                return;
            }
        }

        tooltipPanel.SetActive(false);
    }
}