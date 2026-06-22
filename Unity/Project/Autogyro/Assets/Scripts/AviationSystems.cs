using TMPro;
using UnityEngine;

public class AviationSystems : MonoBehaviour
{
    public GyrocopterFlight flightSystem; // Ссылка на скрипт полета

    [Header("Тумблеры Электросистемы")]
    public CockpitSwitch switchMaster;
    public CockpitSwitch buttonBattery;
    public CockpitSwitch buttonGenerator;
    public CockpitSwitch buttonGeneratorAux;

    [Header("Тумблеры Двигателя и Топлива")]
    public CockpitSwitch buttonEms;
    public CockpitSwitch switchFuelPumpMain;
    public CockpitSwitch switchFuelPumpAux;
    public CockpitSwitch keyIgnition;

    [Header("Параметры работы")]
    public bool isEngineRunning = false;
    public float engineRPM = 0f;
    private float targetRPM = 0f;
    public float maxRPM = 5800f;
    public float idleRPM = 1400f; // Холостой ход реального Rotax

    [Header("UI Ссылка")]
    public TextMeshProUGUI telemetryText;

    void Update()
    {
        // Проверяем глобальное питание бортовой сети
        bool hasPower = switchMaster != null && switchMaster.isOn &&
                        buttonBattery != null && buttonBattery.isOn;

        // Если питание пропало — всё гаснет, двигатель глохнет
        if (!hasPower)
        {
            isEngineRunning = false;
        }

        // Логика оборотов двигателя Rotax 914 Turbo
        if (isEngineRunning && keyIgnition != null && keyIgnition.isOn)
        {
            // ИСПРАВЛЕНО: Теперь целевые обороты зависят от положения газа в скрипте полета!
            if (flightSystem != null)
            {
                // Обороты плавно растут от 1400 (при газе 0) до 5800 (при газе 100%)
                targetRPM = Mathf.Lerp(idleRPM, maxRPM, flightSystem.throttle);
            }
            else
            {
                targetRPM = idleRPM;
            }
        }
        else
        {
            isEngineRunning = false; // Глохнет, если выключили зажигание ключом
            targetRPM = 0f;
        }

        // Сглаживание набора оборотов двигателя (сделаем чуть быстрее и отзывчивее)
        engineRPM = Mathf.MoveTowards(engineRPM, targetRPM, Time.deltaTime * 1800f);

        // Обновляем светлый HUD
        if (telemetryText != null && telemetryText.gameObject.activeInHierarchy)
        {
            UpdateTelemetryUI(hasPower);
        }
    }

    public bool TryStartEngine()
    {
        bool hasPower = switchMaster != null && switchMaster.isOn && buttonBattery != null && buttonBattery.isOn;
        bool emsActive = buttonEms != null && buttonEms.isOn;
        bool fuelActive = switchFuelPumpMain != null && switchFuelPumpMain.isOn;
        bool ignitionActive = keyIgnition != null && keyIgnition.isOn;

        if (hasPower && emsActive && fuelActive && ignitionActive && !isEngineRunning)
        {
            isEngineRunning = true;
            engineRPM = idleRPM; // Сразу выходим на холостые при старте
            return true; // Успешный запуск
        }

        return false; // Чеклист не выполнен
    }

    private void UpdateTelemetryUI(bool hasPower)
    {
        if (!hasPower)
        {
            telemetryText.text = "<b>БОРТОВАЯ ТЕЛЕМЕТРИЯ</b>\n" +
                                 "\n" +
                                 "<color=#DE350B><b>НЕТ ПИТАНИЯ БОРТСЕТИ</b></color>\n" +
                                 "Включите: Switch_Master и Button_Battery\n" +
                                 "";
            return;
        }

        string onStr = "<color=#00875A><b>ВКЛ</b></color>";
        string offStr = "<color=#DE350B><b>ВЫКЛ</b></color>";

        // Берем данные из физики полета, если она привязана
        float alt = flightSystem != null ? flightSystem.GetAltitude() : 0f;
        float speed = flightSystem != null ? flightSystem.forwardSpeed : 0f;
        float rRpm = flightSystem != null ? flightSystem.rotorRPM : 0f;

        telemetryText.text = "<b>ПИЛОТАЖНЫЕ ПРИБОРЫ М-24</b>\n" +
                             "───────────────────────\n" +
                             $"ВЫСОТА: <color=#0066CC><b>{alt:F1} м</b></color>\n" +
                             $"СКОРОСТЬ: <color=#0066CC><b>{(speed * 3.6f):F0} км/ч</b></color>\n" +
                             $"ОБОРОТЫ РОТОРА: <color=#0066CC><b>{(int)rRpm} RPM</b></color>\n" +
                             "───────────────────────\n" +
                             "<b>СТАТУС СИСТЕМ КАБИНЫ</b>\n" +
                             $"СИСТЕМА EMS: {(buttonEms.isOn ? onStr : offStr)}\n" +
                             $"ОСН. НАСОС: {(switchFuelPumpMain.isOn ? onStr : offStr)}\n" +
                             $"ДОП. НАСОС: {(switchFuelPumpAux.isOn ? onStr : offStr)}\n" +
                             $"ЗАЖИГАНИЕ (КЛЮЧ): {(keyIgnition.isOn ? onStr : offStr)}\n" +
                             "\n" +
                             $"ДВИГАТЕЛЬ: {(isEngineRunning ? "<color=#00875A><b>ЗАПУЩЕН</b></color>" : offStr)}\n" +
                             $"ОБОРОТЫ ДВИГ.: <color=#0066CC><b>{(int)engineRPM}</b></color> / {maxRPM} RPM\n" +
                             "\n" +
                             "<i>Управление: Колесико - Газ, Мышь Y - Нос, A/D - Поворот. Space - Предраскрутка винта.</i>";
    }
}