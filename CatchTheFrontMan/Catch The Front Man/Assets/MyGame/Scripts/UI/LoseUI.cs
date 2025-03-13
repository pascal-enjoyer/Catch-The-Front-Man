using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoseUI : MonoBehaviour
{

    public Button restartButton;
    public Button reviveButton;
    public Button menuButton;

    public UnityEvent PlayerRevived;

    public void Start()
    {
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        reviveButton.onClick.AddListener(OnReviveButtonClicked);
        menuButton.onClick.AddListener(OnMenuButtonClicked);
    }

    public void OnRestartButtonClicked()
    {
        GameSettings.Instance.RestartLevel();
    }

    public void OnReviveButtonClicked()
    {
        PlayerRevived.Invoke();
    }

    public void OnMenuButtonClicked()
    {
        GameSettings.Instance.ReturnToMainMenu();
    }
}
