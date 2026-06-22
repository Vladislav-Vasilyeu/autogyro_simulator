using UnityEngine;
using TMPro; // Подключаем TextMeshPro

public class TableManager : MonoBehaviour
{
    public PracticeManager practiceManager;
    public GameObject tablePanel;     // Ссылка на окно таблицы

    [System.Serializable]
    public struct TableRow
    {
        public TextMeshProUGUI cycleText;
        public TextMeshProUGUI rpmText;
        public TextMeshProUGUI speedText;
        public TextMeshProUGUI altitudeText;
        public TextMeshProUGUI timeText;
    }

    [Header("Массив из 4-х строк твоей UI таблицы")]
    public TableRow[] rows = new TableRow[4];

    private int currentSaveRowIndex = 0;

    void Start()
    {
        ClearTable();
    }

    // Кнопка «ЗАПИСЬ»
    public void OnSaveButtonClick()
    {
        // Записываем данные только если текущий полет успешно завершен
        if (practiceManager.currentStage != PracticeManager.PracticeStage.FinishedResult) return;

        if (currentSaveRowIndex >= 4)
        {
            Debug.LogWarning("Таблица заполнена! Все 4 цикла пройдены.");
            return;
        }

        // Автоматически вытягиваем ВСЕ данные из физики и таймеров автожира
        int cycle = practiceManager.GetCycleCounter();
        float rpm = practiceManager.GetTakeoffRPM();
        float speed = practiceManager.GetTakeoffSpeed();
        float altitude = practiceManager.GetMaxAltitude();
        float autoTime = practiceManager.GetPracticeTimer(); // Ввод руками больше не нужен!

        // Записываем в ячейки текущей строки таблицы
        rows[currentSaveRowIndex].cycleText.text = cycle.ToString();
        rows[currentSaveRowIndex].rpmText.text = $"{rpm:F0} RPM";
        rows[currentSaveRowIndex].speedText.text = $"{(speed * 3.6f):F0} км/ч";
        rows[currentSaveRowIndex].altitudeText.text = $"{altitude:F1} м";
        rows[currentSaveRowIndex].timeText.text = $"{autoTime:F1} с"; // Автоматическая запись секунд

        currentSaveRowIndex++;

        // Мгновенный автосброс к ангару для начала новой попытки!
        practiceManager.ResetPracticeSession();
    }

    // Кнопка «ТАБЛИЦА» (Показать/Скрыть)
    public void OnToggleTableButtonClick()
    {
        if (tablePanel != null)
        {
            tablePanel.SetActive(!tablePanel.activeSelf);
        }
    }

    // Кнопка «ОЧИСТКА»
    public void OnClearButtonClick()
    {
        ClearTable();
    }

    void ClearTable()
    {
        currentSaveRowIndex = 0;
        foreach (var row in rows)
        {
            if (row.cycleText != null) row.cycleText.text = "-";
            if (row.rpmText != null) row.rpmText.text = "-";
            if (row.speedText != null) row.speedText.text = "-";
            if (row.altitudeText != null) row.altitudeText.text = "-";
            if (row.timeText != null) row.timeText.text = "-";
        }
    }
}