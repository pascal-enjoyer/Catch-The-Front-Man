using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    public UnityEvent LevelCompleted;

    public void OnLevelCompleted()
    {
        GameSettings.Instance.CompleteLevel(GameSettings.Instance.currentLevel + 1);
        LevelCompleted?.Invoke();
    }
}
