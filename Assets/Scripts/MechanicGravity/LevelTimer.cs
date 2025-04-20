using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    [Header("Установка времени уровня (секунд)")]
    public float levelTime = 180f; // 3 минуты по умолчанию
    [Header("Ссылка на UI текст таймера")]
    public Text timerText;

    private float timeLeft;
    private bool timerRunning = true;

    void Start()
    {
        // Инициализация оставшегося времени
        timeLeft = levelTime;
        UpdateTimerUI(timeLeft);
    }

    void Update()
    {
        if (!timerRunning) return;

        // Отнимаем время
        timeLeft -= Time.deltaTime;
        timeLeft = Mathf.Max(0f, timeLeft);

        UpdateTimerUI(timeLeft);

        if (timeLeft <= 0f)
        {
            timerRunning = false;
            OnTimeEnd();
        }
    }

    private void UpdateTimerUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void OnTimeEnd()
    {
        Debug.Log("Time's up!");
        // Перезагрузить текущую сцену или переход по логике
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Останавливает таймер и возвращает оставшееся время в секундах (целое число).
    /// </summary>
    public int StopTimerAndGetRemainingSeconds()
    {
        timerRunning = false;
        return Mathf.FloorToInt(timeLeft);
    }

    /// <summary>
    /// Останавливает таймер и возвращает оставшееся время в секундах (с дробной частью).
    /// </summary>
    public float StopTimerAndGetRemainingTime()
    {
        timerRunning = false;
        return timeLeft;
    }
}

/*
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    public float levelTime = 300f;
    public Text timerText;
    private float timeLeft;
    private bool timerRunning = true;

    void Start()
    {
        timeLeft = levelTime;
    }

    void Update()
    {
        if (!timerRunning) return;

        timeLeft -= Time.deltaTime;
        timeLeft = Mathf.Max(0f, timeLeft);

        UpdateTimerUI(timeLeft);

        if (timeLeft <= 0f)
        {
            timerRunning = false;
            OnTimeEnd();
        }
    }

    void UpdateTimerUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void OnTimeEnd()
    {
        Debug.Log("Time's up!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

*/