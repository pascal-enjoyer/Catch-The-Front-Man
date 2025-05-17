using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathTimerUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Text timerText; // Для Unity UI Text
    [SerializeField] private TextMeshProUGUI timerTextTMP; // Для TextMeshPro
    [SerializeField] private string timeFormat = "{0:F1} s"; // Формат отображения времени (например, "3.0 s")

    private bool isTimerActive = false;

    private void Start()
    {
        // Подписываемся на события DeathTimer
        DeathTimer.OnTimerStarted += OnTimerStarted;
        DeathTimer.OnTimerEnded += OnTimerEnded;

        // Изначально скрываем текст
        SetTextVisibility(false);
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        DeathTimer.OnTimerStarted -= OnTimerStarted;
        DeathTimer.OnTimerEnded -= OnTimerEnded;
    }

    private void Update()
    {
        if (isTimerActive && DeathTimer.IsTimerActive)
        {
            // Вычисляем оставшееся время
            float timeLeft = DeathTimer.GetRemainingTime();
            UpdateTimerText(timeLeft);
        }
    }

    private void OnTimerStarted()
    {
        isTimerActive = true;
        SetTextVisibility(true);
        Debug.Log("DeathTimerUI: Timer started, text enabled.");
    }

    private void OnTimerEnded()
    {
        isTimerActive = false;
        SetTextVisibility(false);
        Debug.Log("DeathTimerUI: Timer ended, text disabled.");
    }

    private void SetTextVisibility(bool isVisible)
    {
        if (timerText != null)
        {
            timerText.enabled = isVisible;
        }
        if (timerTextTMP != null)
        {
            timerTextTMP.enabled = isVisible;
        }
    }

    private void UpdateTimerText(float timeLeft)
    {
        // Форматируем время (например, "3.0 s")
        string text = string.Format(timeFormat, Mathf.Max(0, timeLeft));
        if (timerText != null)
        {
            timerText.text = text;
        }
        if (timerTextTMP != null)
        {
            timerTextTMP.text = text;
        }
    }
}