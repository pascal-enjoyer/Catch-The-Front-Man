using System.Collections.Generic;
using UnityEngine;

public class LevelsManager : MonoBehaviour
{
    public List<LevelUI> levels = new List<LevelUI>();

    public void Awake()
    {
        foreach (LevelUI level in levels)
        {
            level.LevelClicked.AddListener(OnLevelButtonClicked);
            level.ToggleLevelButton(level.levelNumber <= GameSettings.Instance.completedLevels);
        }
    }

    public void OnLevelButtonClicked(int levelNumber)
    {
        GameSettings.Instance.LoadLevelScene(levelNumber);
    }
}
