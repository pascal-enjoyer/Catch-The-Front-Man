using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    public GameObject lockIcon;
    public GameObject levelText;
    public Button levelButton;

    public UnityEvent<int> LevelClicked;
    public int levelNumber;

    public void Start()
    {
        levelButton.onClick.AddListener(OnLevelButtonClicked);
    }
     
    public void ToggleLevelButton(bool IsOn)
    {
        levelButton.enabled = IsOn;
        lockIcon.SetActive(!IsOn);
    }


    public void OnLevelButtonClicked()
    {
        LevelClicked?.Invoke(levelNumber);
    }


}
