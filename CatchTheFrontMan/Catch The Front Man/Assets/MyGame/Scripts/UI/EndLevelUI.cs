using UnityEngine;
using UnityEngine.UI;

public class EndLevelUI : MonoBehaviour
{
    public GameObject endLevelPanel;

    public void Start()
    {
        GameSettings.Instance.LevelEnded.AddListener(ShowEndLevelUI);
    }

    public void ShowEndLevelUI()
    {
        endLevelPanel.SetActive(true);
    }

    public void OnNextLevelButtonClicked()
    {
        GameSettings.Instance.OnNextLevelButtonClicked();
    }

    public void OnMainMenuButtonClicked()
    {
        GameSettings.Instance.ReturnToMainMenu();
    }
}
