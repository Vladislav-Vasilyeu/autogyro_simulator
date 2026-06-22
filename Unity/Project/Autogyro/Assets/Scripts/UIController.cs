using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Контейнеры Интерфейса")]
    public GameObject mainMenuContainer;
    public GameObject simulationHUDContainer;

    [Header("Элементы Справки (?)")]
    public GameObject helpPopupPanel;
    public TextMeshProUGUI helpText;

    [Header("Нижняя Панель описания")]
    public TextMeshProUGUI bottomInfoText;

    [Header("Ссылка на внешнюю камеру")]
    public Camera_moving externalCamera;

    [Header("Ссылки на 3D объекты (Highlight)")]
    public HighlightElement rotorMesh;
    public HighlightElement propellerMesh;
    public HighlightElement dashboardMesh;
    public HighlightElement landingGearMesh;
    public HighlightElement fuselageMesh;

    private bool isPracticeMode = false;

    void Start()
    {
        mainMenuContainer.SetActive(true);
        simulationHUDContainer.SetActive(false);
        helpPopupPanel.SetActive(false);

        bottomInfoText.text = "Режим ознакомления. Наведите курсор на элементы установки справа...";

        helpText.text = "<b>ИНСТРУКЦИЯ СИМУЛЯТОРA:</b>\n\n" +
                        "1. Зажмите <b>Left Alt</b>, чтобы освободить курсор мыши.\n" +
                        "2. Включайте тумблеры по чеклисту: Батарея -> Зажигание -> Насосы.\n" +
                        "3. Нажмите кнопку СТАРТЕР для запуска Rotax 914.\n" +
                        "4. Колесико мыши управляет уровнем газа.\n" +
                        "5. Клавиши <b>A / D</b> управляют хвостовым рулем.";
    }

    public void OnHelpPointerEnter()
    {
        Debug.Log("Наведение Help");
        helpPopupPanel.SetActive(true);
    }
    public void OnHelpPointerExit()
    {
        Debug.Log("Убрали курсор Help");
        helpPopupPanel.SetActive(false);
    }

    // --- Наведение мыши на обновленные кнопки (Hover) ---
    public void OnElementPointerEnter(string elementName)
    {
        if (isPracticeMode) return;

        switch (elementName)
        {
            case "Rotor":
                bottomInfoText.text = "<b>1. Несущий винт (Ротор):</b> Двухлопастный винт. Обеспечивает подъемную силу за счет эффекта авторотации (вращения набегающим потоком воздуха).";
                if (rotorMesh != null) rotorMesh.SetHighlight(true);
                break;
            case "Propeller":
                bottomInfoText.text = "<b>2. Толкающий винт:</b> Трехлопастный винт с изменяемым шагом. Соединен с двигателем Rotax 914 и создает горизонтальную тягу.";
                if (propellerMesh != null) propellerMesh.SetHighlight(true);
                break;
            case "Dashboard":
                bottomInfoText.text = "<b>3. Приборная панель:</b> Содержит пилотажно-навигационные приборы, указатель оборотов ротора (ротор тахометр) и тумблеры запуска систем.";
                if (dashboardMesh != null) dashboardMesh.SetHighlight(true);
                break;
            case "LandingGear":
                bottomInfoText.text = "<b>4. Шасси:</b> Трехопорное, с носовым управляющим колесом. Оснащено гидравлическими тормозами и амортизацией для мягкой посадки.";
                if (landingGearMesh != null) landingGearMesh.SetHighlight(true);
                break;
            case "Fuselage":
                bottomInfoText.text = "<b>5. Корпус (Фюзеляж):</b> Выполнен из прочного углепластика (карбона). Защищает экипаж и задает идеальную аэродинамическую форму автожира.";
                if (fuselageMesh != null) fuselageMesh.SetHighlight(true);
                break;
        }
    }

    public void OnElementPointerExit(string elementName)
    {
        if (isPracticeMode) return;

        bottomInfoText.text = "Режим ознакомления. Наведите курсор на элементы установки справа...";

        if (rotorMesh != null) rotorMesh.SetHighlight(false);
        if (propellerMesh != null) propellerMesh.SetHighlight(false);
        if (dashboardMesh != null) dashboardMesh.SetHighlight(false);
        if (landingGearMesh != null) landingGearMesh.SetHighlight(false);
        if (fuselageMesh != null) fuselageMesh.SetHighlight(false);
    }

    // --- Клик по кнопкам: новые ракурсы для твоей камеры Camera_moving ---
    public void OnElementClick(string elementName)
    {
        if (isPracticeMode || externalCamera == null) return;

        switch (elementName)
        {
            case "Rotor":
                externalCamera.SetCameraFocus(140f, 45f, 6f, new Vector3(0f, 1.5f, 0f));
                break;
            case "Propeller":
                externalCamera.SetCameraFocus(180f, 15f, 4.5f, new Vector3(0f, 0f, -1.5f));
                break;
            case "Dashboard":
                externalCamera.SetCameraFocus(160f, 12f, 0.2f, new Vector3(0f, 0f, -1.4f));
                break;
            case "LandingGear":
                externalCamera.SetCameraFocus(120f, -7f, 4f, new Vector3(0f, -0.6f, 0f));
                break;
            case "Fuselage":
                externalCamera.SetCameraFocus(90f, 15f, 7.5f, new Vector3(0f, 0.2f, 0f));
                break;
        }
    }

    public void StartPracticeMode()
    {
        isPracticeMode = true;
        mainMenuContainer.SetActive(false);
        simulationHUDContainer.SetActive(true);
        if (externalCamera != null) externalCamera.ResetCameraFocus();
    }
}
