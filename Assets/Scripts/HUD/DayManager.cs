using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    [Header("configuração do dia")]
    [SerializeField] private int dayNumber = 1;
    [SerializeField] private float dayDuration = 300f; // 5 minutos
    [SerializeField] private int targetScore = 50;
    [SerializeField] private int pointsPerOrder = 10;

    [Header("cenas")]
    [SerializeField] private string nextLevelSceneName = "";
    [SerializeField] private string failSceneName = "";

    [Header("ui")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;

    private float currentTime;
    private int currentScore = 0;
    private bool dayEnded = false;

    void Start()
    {
        currentTime = dayDuration;
        UpdateUI();
    }

    void Update()
    {
        if (dayEnded)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            EndDay();
        }

        UpdateUI();
    }

    public void AddOrderPoints()
    {
        if (dayEnded)
            return;

        currentScore += pointsPerOrder;

        Debug.Log("pontos atuais: " + currentScore);

        UpdateUI();
    }

    private void EndDay()
    {
        dayEnded = true;

        Debug.Log("fim do dia");

        if (currentScore >= targetScore)
        {
            Debug.Log("objetivo atingido, passar nivel");

            if (nextLevelSceneName != "")
            {
                SceneManager.LoadScene(nextLevelSceneName);
            }
        }
        else
        {
            Debug.Log("objetivo falhou");

            if (failSceneName != "")
            {
                SceneManager.LoadScene(failSceneName);
            }
        }
    }

    private void UpdateUI()
    {
        if (dayText != null)
        {
            dayText.text = "Dia " + dayNumber;
        }

        if (scoreText != null)
        {
            scoreText.text = "Pontos: " + currentScore + " / " + targetScore;
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);

            timerText.text = "Tempo: " + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
    }
}