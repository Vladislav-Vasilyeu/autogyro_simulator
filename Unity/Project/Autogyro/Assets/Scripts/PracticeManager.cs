using UnityEngine;
using TMPro; // Подключаем TextMeshPro

public class PracticeManager : MonoBehaviour
{
    public enum PracticeStage { EngineStart, TaxiToRunway, PreRotation, Takeoff, Checkpoints, FinishedResult }

    [Header("Текущий этап")]
    public PracticeStage currentStage = PracticeStage.EngineStart;

    [Header("UI Элементы Панели Практики")]
    public TextMeshProUGUI hintText;         // Окно подсказок практики (TMP)
    public GameObject tablePanel;           // Сама панель таблицы

    [Header("Ссылки на Системы Автожира")]
    public GyrocopterFlight gyrocopter;
    public AviationSystems aviation;

    [Header("Маршрут (Чекпоинты)")]
    public Transform[] checkpoints;
    private int currentCheckpointIndex = 0;

    // Начальная позиция автожира у ангара
    private Vector3 hangarPosition;
    private Quaternion hangarRotation;

    // Параметры для записи в таблицу
    private int cycleCounter = 0;
    private float practiceTimer = 0f;
    private float maxAltitude = 0f;
    private float takeoffSpeed = 0f;
    private float takeoffRPM = 0f;
    private bool isTakeoffRecorded = false;

    void Start()
    {
        // Запоминаем точку старта автожира при запуске сцены
        if (gyrocopter != null)
        {
            hangarPosition = gyrocopter.transform.position;
            hangarRotation = gyrocopter.transform.rotation;
        }
        ResetPracticeSession();
    }

    // Метод полного сброса к ангару
    public void ResetPracticeSession()
    {
        if (gyrocopter == null) return;

        // 1. Возвращаем позицию и гасим физику
        gyrocopter.transform.position = hangarPosition;
        gyrocopter.transform.rotation = hangarRotation;

        Rigidbody rb = gyrocopter.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 2. Глушим системы и тумблеры (из твоего скрипта AviationSystems)
        if (aviation != null)
        {
            aviation.isEngineRunning = false;
            aviation.engineRPM = 0f;
            if (aviation.switchMaster != null) aviation.switchMaster.isOn = false;
            if (aviation.buttonBattery != null) aviation.buttonBattery.isOn = false;
            if (aviation.keyIgnition != null) aviation.keyIgnition.isOn = false;
        }

        // 3. Сбрасываем параметры полета
        gyrocopter.rotorRPM = 0f;

        // 4. Сброс таймеров и чекпоинтов
        practiceTimer = 0f;
        maxAltitude = 0f;
        isTakeoffRecorded = false;
        currentCheckpointIndex = 0;

        ToggleCheckpointsVisual(false);

        // Возврат к первому заданию
        SetStage(PracticeStage.EngineStart);
    }

    // Вызывается при нажатии на кнопки "Задание 1 - 5"
    public void SelectTask(int taskNumber)
    {
        if (currentStage == PracticeStage.FinishedResult) return;

        switch (taskNumber)
        {
            case 1: currentStage = PracticeStage.EngineStart; ToggleCheckpointsVisual(false); break;
            case 2: currentStage = PracticeStage.TaxiToRunway; ToggleCheckpointsVisual(false); break;
            case 3: currentStage = PracticeStage.PreRotation; ToggleCheckpointsVisual(false); break;
            case 4: currentStage = PracticeStage.Takeoff; ToggleCheckpointsVisual(false); break;
            case 5: currentStage = PracticeStage.Checkpoints; ToggleCheckpointsVisual(true); break;
        }
        UpdateHintText();
    }

    void Update()
    {
        if (currentStage == PracticeStage.FinishedResult) return;

        // Таймер идет, пока мы выполняем задания
        practiceTimer += Time.deltaTime;

        // Считаем высоту относительно земли (Y)
        float currentAlt = gyrocopter.transform.position.y;
        if (currentAlt > maxAltitude) maxAltitude = currentAlt;

        // Автоматическая проверка выполнения этапов
        switch (currentStage)
        {
            case PracticeStage.EngineStart:
                if (aviation != null && aviation.isEngineRunning) SetStage(PracticeStage.TaxiToRunway);
                break;

            case PracticeStage.TaxiToRunway:
                if (Vector3.Distance(gyrocopter.transform.position, hangarPosition) > 35f && gyrocopter.forwardSpeed < 2f)
                {
                    SetStage(PracticeStage.PreRotation);
                }
                break;

            case PracticeStage.PreRotation:
                if (gyrocopter.rotorRPM >= 240f) SetStage(PracticeStage.Takeoff);
                break;

            case PracticeStage.Takeoff:
                if (currentAlt > 5f)
                {
                    if (!isTakeoffRecorded)
                    {
                        takeoffSpeed = gyrocopter.forwardSpeed;
                        takeoffRPM = gyrocopter.rotorRPM;
                        isTakeoffRecorded = true;
                    }
                    SetStage(PracticeStage.Checkpoints);
                    ToggleCheckpointsVisual(true);
                }
                break;

            case PracticeStage.Checkpoints:
                if (checkpoints.Length > 0 && currentCheckpointIndex < checkpoints.Length)
                {
                    float distance = Vector3.Distance(gyrocopter.transform.position, checkpoints[currentCheckpointIndex].position);
                    if (distance < 25f)
                    {
                        checkpoints[currentCheckpointIndex].gameObject.SetActive(false);
                        currentCheckpointIndex++;

                        if (currentCheckpointIndex >= checkpoints.Length)
                        {
                            cycleCounter++;
                            SetStage(PracticeStage.FinishedResult);
                        }
                        else
                        {
                            UpdateHintText();
                        }
                    }
                }
                break;
        }
    }

    void SetStage(PracticeStage stage)
    {
        currentStage = stage;
        UpdateHintText();
    }

    void UpdateHintText()
    {
        if (hintText == null) return;

        switch (currentStage)
        {
            case PracticeStage.EngineStart:
                hintText.text = "<color=#FFA500><b>ЗАДАНИЕ 1: ЗАПУСК</b></color>\nВключите тумблеры питания (Мастер, Батарея) и запустите Ротакс кнопкой стартера.";
                break;
            case PracticeStage.TaxiToRunway:
                hintText.text = "<color=#FFA500><b>ЗАДАНИЕ 2: РУЛЕНИЕ</b></color>\nАккуратно вырулите от ангара вперед на взлетно-посадочную полосу (ВПП).";
                break;
            case PracticeStage.PreRotation:
                hintText.text = "<color=#FFA500><b>ЗАДАНИЕ 3: РАСКРУТКА</b></color>\nЗажмите клавишу раскрутки [Space] и удерживайте до достижения 240+ RPM ротора.";
                break;
            case PracticeStage.Takeoff:
                hintText.text = "<color=#FFA500><b>ЗАДАНИЕ 4: ВЗЛЕТ</b></color>\nУстановите максимальный маршевый газ и плавно потяните РУС (мышь) на себя для взлета.";
                break;
            case PracticeStage.Checkpoints:
                hintText.text = $"<color=#FFA500><b>ЗАДАНИЕ 5: МАРШРУТ</b></color>\nПройдите контрольные точки в небе.\nОсталось: <color=#0066CC><b>{currentCheckpointIndex} из {checkpoints.Length}</b></color>";
                break;
            case PracticeStage.FinishedResult:
                hintText.text = $"<color=#00875A><b>ПОЛЕТ ЗАВЕРШЕН (Цикл №{cycleCounter})!</b></color>\n\n" +
                                $"Время симуляции: {practiceTimer:F1} сек.\n" +
                                $"Макс. высота: {maxAltitude:F1} м.\n" +
                                $"<i>Все параметры зафиксированы. Нажмите кнопку 'Запись' для переноса данных в таблицу результатов.</i>";
                break;
        }
    }

    void ToggleCheckpointsVisual(bool show)
    {
        foreach (var cp in checkpoints)
        {
            if (cp != null) cp.gameObject.SetActive(show);
        }
    }

    // Публичные геттеры для таблицы
    public int GetCycleCounter() => cycleCounter;
    public float GetTakeoffRPM() => takeoffRPM;
    public float GetTakeoffSpeed() => takeoffSpeed;
    public float GetMaxAltitude() => maxAltitude;
    public float GetPracticeTimer() => practiceTimer; // Забираем время напрямую из таймера!
}