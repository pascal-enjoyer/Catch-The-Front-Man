using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathTimerUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Text timerText; // ��� Unity UI Text
    [SerializeField] private TextMeshProUGUI timerTextTMP; // ��� TextMeshPro
    [SerializeField] private string timeFormat = "{0:F1} s"; // ������ ����������� ������� (��������, "3.0 s")

    private bool isTimerActive = false;

    private void Start()
    {
        // ������������� �� ������� DeathTimer
        DeathTimer.OnTimerStarted += OnTimerStarted;
        DeathTimer.OnTimerEnded += OnTimerEnded;

        // ���������� �������� �����
        SetTextVisibility(false);
    }

    private void OnDestroy()
    {
        // ������������ �� �������
        DeathTimer.OnTimerStarted -= OnTimerStarted;
        DeathTimer.OnTimerEnded -= OnTimerEnded;
    }

    private void Update()
    {
        if (isTimerActive && DeathTimer.IsTimerActive)
        {
            // ��������� ���������� �����
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
        // ����������� ����� (��������, "3.0 s")
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