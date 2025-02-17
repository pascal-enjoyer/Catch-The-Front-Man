using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;
    public int currentLevel = 0;
    public int completedLevels = 1;
    private void Awake()
    {
        Application.targetFrameRate = Mathf.Max(60, (int)(Screen.currentResolution.refreshRateRatio.value));

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings(); // Загрузка настроек при запуске
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        SaveSettings(); // Сохранение настроек при выходе из игры
    }


    public void LoadLevelScene(int level)
    {
        currentLevel = level;
        SceneManager.LoadScene(level);
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(0);
    }

    public void LeaveFromLevel()
    {
        LoadMainMenuScene();
    }

    public void RestartLevel()
    {
        LoadLevelScene(currentLevel);
    }


    private void LoadSettings()
    {
    }

    private void SaveSettings()
    {
        PlayerPrefs.Save();
    }
}
