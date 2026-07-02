using TMPro;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [Header("configura��o do dia")]
    [SerializeField] private int dayNumber = 1;
    [SerializeField] private float dayDuration = 300f;
    [SerializeField] private int targetScore = 100;

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

    [Header("áudio")]
    [SerializeField] private AudioClip gameplayBackgroundMusic;

    private float currentTime;
    private int currentScore;
    private bool dayRunning = false;
    private bool dayEnded = false;

    void Start()
    {
        currentTime = dayDuration;
        currentScore = 0;
        dayRunning = false;
        dayEnded = false;

        // Toca a música de fundo do jogo
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

    public void StartDay()
    {
        if (dayEnded)
            return;

        dayRunning = true;
        Debug.Log("o dia come�ou e o timer arrancou");
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

        // Para a música de fundo
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBackgroundMusic();
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
            dayEndPanel.ShowResult(success, currentScore, targetScore);
        }

        if (success)
        {
            Debug.Log("dia conclu�do com sucesso");
        }
        else
        {
            Debug.Log("n�o atingiste a pontua��o necess�ria");
        }
    }
}
