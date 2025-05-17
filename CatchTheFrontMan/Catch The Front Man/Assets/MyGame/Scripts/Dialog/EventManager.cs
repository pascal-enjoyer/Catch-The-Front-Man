using System;

public static class EventManager
{
    public static event Action OnDialogStarted;
    public static event Action OnDialogEnded;

    public static void RaiseDialogStarted() => OnDialogStarted?.Invoke();
    public static void RaiseDialogEnded() => OnDialogEnded?.Invoke();
}