using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public static Timer instance;
    public float currentTime = 0f;
    public bool timerIsRunning = false;
    
    [SerializeField] private TextMeshProUGUI timeText;

    private void Awake()
    {
        // Singleton pattern with reset capability
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep the timer across scene loads
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Register for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset and restart timer when a new scene is loaded (gameplay scene)
        ResetTimer();
        StartTimer();
        
        // Try to find the timer text if not assigned
        if (timeText == null)
        {
            GameObject timerTextObj = GameObject.Find("TimerText");
            if (timerTextObj != null)
            {
                timeText = timerTextObj.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void Start()
    {
        StartTimer();
    }

    void Update()
    {
        if (timerIsRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            int milliseconds = Mathf.FloorToInt((currentTime * 100) % 100);

            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
    }    public void ResetTimer()
    {
        currentTime = 0f;
        UpdateTimerDisplay();
    }
    
    public void StartTimer()
    {
        timerIsRunning = true;
    }

    public void StopTimer()
    {
        timerIsRunning = false;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        int milliseconds = Mathf.FloorToInt((currentTime * 100) % 100);

        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }
}