using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelsManager : MonoBehaviour
{
    public List<LevelUI> levels = new List<LevelUI>();

    public void Start()
    {
        foreach (LevelUI level in levels)
        {
            level.LevelClicked.AddListener(OnLevelButtonClicked);
            level.ToggleLevelButton(level.levelNumber <= GameSettings.Instance.completedLevels+1);
        }
    }

    public void OnLevelButtonClicked(int levelNumber)
    {
        GameSettings.Instance.LoadLevel(levelNumber);
    }
}
