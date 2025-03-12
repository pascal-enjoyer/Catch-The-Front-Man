using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class PlayerEconomic : MonoBehaviour
{


    [SerializeField] private int score;
    [SerializeField] private int bestScore;

    public int Score { get { return score; } }

    public UnityEvent<int, int> ScoreChanged;
    public UnityEvent<int> BestScoreChanged;

    [SerializeField] private bool isBestRecordChanged = false;

    private string BestScoreKey = "BestScore";

    private void Start()
    {
        LoadScore();
    }

    public void ChangeScoreMode()
    {

    }

    public void StartPlayerScore()
    {

        ChangeScoreMode();
        if (!PlayerPrefs.HasKey(BestScoreKey))
        {
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
        }
        isBestRecordChanged = false;
        LoadScore();
    }

    public void AddScore(int points)
    {
        score += points;
        if (score > bestScore)
        {
            SetBestScore(score);

            SaveScore();
        }
        ScoreChanged.Invoke(score, bestScore);
    }

    private void SetBestScore(int bestScore)
    {
        this.bestScore = bestScore;

        if (!isBestRecordChanged)
        {
            BestScoreChanged.Invoke(bestScore);
            isBestRecordChanged = true;
        }
    }

    public int GetScore()
    {
        return score;
    }

    public int GetBestScore()
    {
        return bestScore;
    }

    private void SaveScore()
    {
        PlayerPrefs.SetInt(BestScoreKey, bestScore);
        PlayerPrefs.Save();
    }

    private void LoadScore()
    {
        score = 0;
        int loadedScore = 0;
        if (PlayerPrefs.HasKey(BestScoreKey))
        {
            loadedScore = PlayerPrefs.GetInt(BestScoreKey);
        }
        bestScore = loadedScore;
        ScoreChanged.Invoke(score, bestScore);
    }

}
