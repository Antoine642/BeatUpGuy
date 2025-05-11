using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI completionTimeText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;    
    private void Awake()
    {
        // Ensure the panel is hidden at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    private void Start()
    {
        // Setup button listeners in Start to ensure they're set up after serialization
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            retryButton.onClick.AddListener(RetryGame);
        }
            
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }    public void ShowGameOver(string completionTime)
    {
        Debug.Log("ShowGameOver called with time: " + completionTime);
        
        // Stop the timer
        if (Timer.instance != null)
            Timer.instance.StopTimer();
        
        // Find the panel if it's not set
        if (gameOverPanel == null)
        {
            gameOverPanel = transform.Find("GameOverPanel")?.gameObject;
            if (gameOverPanel == null)
            {
                Debug.LogError("GameOverPanel not found in hierarchy!");
                return;
            }
        }
            
        // Show the panel
        gameOverPanel.SetActive(true);
        
        // Find the text component if it's not set
        if (completionTimeText == null)
        {
            completionTimeText = gameOverPanel.transform.Find("CompletionTimeText")?.GetComponent<TMPro.TextMeshProUGUI>();
            if (completionTimeText == null)
            {
                Debug.LogError("CompletionTimeText not found in GameOverPanel!");
            }
        }
            
        // Update the completion time text
        if (completionTimeText != null)
            completionTimeText.text = "Your Time: " + completionTime;
            
        // Make sure the buttons are correctly attached
        if (retryButton == null)
        {
            retryButton = gameOverPanel.transform.Find("RetryButton")?.GetComponent<Button>();
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(RetryGame);
            }
        }
            
        if (mainMenuButton == null)
        {
            mainMenuButton = gameOverPanel.transform.Find("MainMenuButton")?.GetComponent<Button>();
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }
        }
    }
      private void RetryGame()
    {
        // Hide the game over panel first
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Use the GameManager to restart if available
        if (GameManager.instance != null)
        {
            GameManager.instance.RestartGame();
        }
        else
        {
            // Fallback if GameManager is not available
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    
    private void ReturnToMainMenu()
    {
        // Hide the game over panel first
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Reset the game state in GameManager if available
        if (GameManager.instance != null)
        {
            var gameManagerType = GameManager.instance.GetType();
            var fieldInfo = gameManagerType.GetField("isGameOver", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (fieldInfo != null)
                fieldInfo.SetValue(GameManager.instance, false);
        }
        
        // Load the main menu scene
        SceneManager.LoadScene(0); // Assuming main menu is scene 0
    }
}