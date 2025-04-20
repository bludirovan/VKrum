using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    public float levelTime = 180f; // 3 минуты
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

