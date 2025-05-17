using UnityEngine;
using System;

public static class DeathTimer
{
    private const float TimerDuration = 3f;
    private static float _timerEndTime = -1f;
    private static bool _isTimerActive = false;

    public static bool IsTimerActive { get => _isTimerActive; private set => _isTimerActive = value; }
    public static event Action OnTimerStarted;
    public static event Action OnTimerEnded;

    public static void StartDeathTimer()
    {
        if (_isTimerActive) return;
        _isTimerActive = true;
        _timerEndTime = Time.time + TimerDuration;
        OnTimerStarted?.Invoke();
        //Debug.Log($"DeathTimer started. Duration: {TimerDuration} seconds. IsTimerActive: {_isTimerActive}");
    }

    public static void UpdateTimer()
    {
        if (!_isTimerActive) return;
        if (Time.time >= _timerEndTime)
        {
            StopDeathTimer();
        }
    }

    public static void StopDeathTimer()
    {
        if (!_isTimerActive) return;
        _isTimerActive = false;
        _timerEndTime = -1f;
        OnTimerEnded?.Invoke();
        //Debug.Log($"DeathTimer ended. IsTimerActive: {_isTimerActive}");
    }

    public static float GetRemainingTime()
    {
        if (!_isTimerActive) return 0f;
        return Mathf.Max(0, _timerEndTime - Time.time);
    }
}