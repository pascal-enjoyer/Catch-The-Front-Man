using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;
    public GameController gameController;

    [Header("Progress Settings")]
    public int currentLevel = 1;
    public int totalLevels = 10;
    public int completedLevels = 0;

    public int totalTutorialLevels = 4;
    public int completedTutorials = 0;
    public bool isTutorialCompleted = false;

    public UnityEvent LevelEnded;

    private bool isInitialized = false;

    private void Awake()
    {

        if (isInitialized) return;
        SetupSingleton();
        InitializeFrameRate();
        LoadSettings();
        SceneManager.sceneLoaded += OnSceneLoaded;

        isInitialized = true;
    }
    private void SetupSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeFrameRate()
    {
        Application.targetFrameRate = Mathf.Clamp(
            Mathf.Max(60, (int)(Screen.currentResolution.refreshRateRatio.value)),
            30,
            144
        );
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) // Главное меню
        {

            if (!isTutorialCompleted)
            {
                LoadNextTutorialLevel();
            }
        }
    }

    private void LoadNextTutorialLevel()
    {
        int nextTutorialIndex = completedTutorials + 1;
        if (nextTutorialIndex <= totalTutorialLevels)
        {
            LoadTutorial(nextTutorialIndex);
        }
    }
    public void OnLevelCompleted()
    {
        if (!isTutorialCompleted)
        {
            CompleteTutorial();
            if (isTutorialCompleted)
            {
                ReturnToMainMenu();
            }
            else
            {
                LoadNextTutorialLevel();
            }
        }
        else
        {
            LevelEnded?.Invoke();
            CompleteLevel(currentLevel);
        }
    }

    public void CompleteLevel(int level)
    {
        // Сохраняем только максимальный пройденный уровень
        completedLevels = Mathf.Max(completedLevels, level);
        SaveSettings();
    }

    public void CompleteTutorial()
    {
        completedTutorials++;
        if (completedTutorials >= totalTutorialLevels)
        {
            isTutorialCompleted = true;
            completedLevels = 0;
        }
        SaveSettings();
    }
    public void LoadTutorial(int tutorialNumber)
    {
        if (tutorialNumber >= 1 && tutorialNumber <= totalTutorialLevels)
        {
            SceneManager.LoadScene(tutorialNumber);
        }
    }
    public void LoadLevel(int level)
    {
        if (isTutorialCompleted)
        {
            // Основные уровни идут после туториалов
            int sceneIndex = totalTutorialLevels + level;

            if (sceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                currentLevel = level;
                SceneManager.LoadScene(sceneIndex);
            }
            else
            {
                Debug.LogError("Level index out of range!");
            }
        }
    }
    public void OnNextLevelButtonClicked()
    {
        if (!isTutorialCompleted)
            LoadNextTutorialLevel();
        else
        {
            LoadLevel(currentLevel +1 );
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadSettings()
    {
        completedLevels = PlayerPrefs.GetInt("CompletedLevels", 0);
        completedTutorials = PlayerPrefs.GetInt("CompletedTutorials", 0);
        isTutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("CompletedLevels", completedLevels);
        PlayerPrefs.SetInt("CompletedTutorials", completedTutorials);
        PlayerPrefs.SetInt("TutorialCompleted", isTutorialCompleted ? 1 : 0);
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
    }
    private void OnApplicationQuit()
    {
        SaveSettings();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveSettings();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}