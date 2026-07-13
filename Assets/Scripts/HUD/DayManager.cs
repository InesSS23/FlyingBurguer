using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    [Header("configuracao do nivel")]
    [SerializeField] private LevelConfig levelConfig;

    [Header("configuracao do dia - fallback se nao houver LevelConfig")]
    [SerializeField] private int dayNumber = 1;
    [SerializeField] private float dayDuration = 300f;
    [SerializeField] private int targetScore = 100;
    [SerializeField] private int pointsPerDelivery = 10;

    [Header("ui")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;

    [Header("fim do dia")]
    [SerializeField] private DayEndPanel dayEndPanel;
    [SerializeField] private FirstPersonPlayer firstPersonPlayer;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private CustomerManager customerManager;
    [SerializeField] private PauseMenuController pauseMenuController;

    [Header("audio")]
    [SerializeField] private AudioClip gameplayBackgroundMusic;

    private float currentTime;
    private int currentScore;
    private bool dayRunning = false;
    private bool dayEnded = false;

    void Start()
    {
        Time.timeScale = 1f;

        ApplyLevelConfig();
        SaveProgressForThisLevel();

        currentTime = dayDuration;
        currentScore = 0;
        dayRunning = false;
        dayEnded = false;

        if (customerManager != null && levelConfig != null)
        {
            customerManager.ApplyLevelConfig(levelConfig);
        }

        if (AudioManager.Instance != null && gameplayBackgroundMusic != null)
        {
            AudioManager.Instance.PlayBackgroundMusic(gameplayBackgroundMusic);
        }

        UpdateUI();
    }

    void Update()
    {
        if (!dayRunning || dayEnded)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            dayRunning = false;
            dayEnded = true;

            UpdateUI();
            FinishDay();

            return;
        }

        UpdateUI();
    }

    private void ApplyLevelConfig()
    {
        if (levelConfig == null)
            return;

        dayNumber = levelConfig.dayNumber;
        dayDuration = levelConfig.dayDuration;
        targetScore = levelConfig.targetScore;
        pointsPerDelivery = levelConfig.pointsPerDelivery;

        if (levelConfig.gameplayBackgroundMusic != null)
        {
            gameplayBackgroundMusic = levelConfig.gameplayBackgroundMusic;
        }
    }

    private void SaveProgressForThisLevel()
    {
        if (levelConfig != null && !string.IsNullOrWhiteSpace(levelConfig.sceneName))
        {
            LevelProgress.SaveCurrentLevel(levelConfig.sceneName);
            return;
        }

        LevelProgress.SaveCurrentLevel(SceneManager.GetActiveScene().name);
    }

    public LevelConfig GetLevelConfig()
    {
        return levelConfig;
    }

    public void StartDay()
    {
        if (dayEnded)
            return;

        dayRunning = true;
        Debug.Log("o dia comecou e o timer arrancou");
    }

    public void StopDay()
    {
        dayRunning = false;
    }

    public void AddScore(int amount)
    {
        if (dayEnded)
            return;

        currentScore = Mathf.Max(0, currentScore + amount);
        UpdateUI();

        Debug.Log("pontos atuais: " + currentScore);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public int GetPointsPerDelivery()
    {
        return pointsPerDelivery;
    }

    public bool IsDayEnded()
    {
        return dayEnded;
    }

    private void UpdateUI()
    {
        if (dayText != null)
        {
            dayText.text = "Dia " + dayNumber;
        }

        if (scoreText != null)
        {
            scoreText.text = currentScore + " / " + targetScore;
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);

            timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        }
    }

    private void FinishDay()
    {
        bool success = currentScore >= targetScore;

        dayRunning = false;
        dayEnded = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBackgroundMusic();
            AudioManager.Instance.StopAllLoopingSFX();
        }

        if (pauseMenuController != null)
        {
            pauseMenuController.ForceClosePauseMenu();
        }

        if (firstPersonPlayer != null)
            firstPersonPlayer.enabled = false;

        if (playerInteraction != null)
            playerInteraction.enabled = false;

        if (customerManager != null)
            customerManager.StopSpawning();

        if (dayEndPanel != null)
        {
            dayEndPanel.ShowResult(success, currentScore, targetScore, levelConfig);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        if (success)
        {
            Debug.Log("dia concluido com sucesso");
        }
        else
        {
            Debug.Log("nao atingiste a pontuacao necessaria");
        }
    }
}