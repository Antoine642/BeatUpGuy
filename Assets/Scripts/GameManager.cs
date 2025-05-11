using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    // Reference to UI elements
    [SerializeField] private GameOverUI gameOverUI;

    // Game state flags
    private bool isGameOver = false;
    private bool isSceneTransitioning = false;
    private bool isGameFrozen = false;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this method when the player completes the level
    public void CompleteLevel()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            
            // Get the completion time from timer
            string completionTime = "00:00:00";
            if (Timer.instance != null)
            {
                completionTime = Timer.instance.GetFormattedTime();
            }
            
            // Show game over UI
            if (gameOverUI != null)
            {
                gameOverUI.ShowGameOver(completionTime);
            }
            else
            {
                Debug.LogWarning("GameOverUI reference not set in GameManager");
            }
        }
    }    // Call this method when the player dies or fails
    public void GameOver()
    {
        if (!isGameOver && !isSceneTransitioning)
        {
            isGameOver = true;
            
            // Freeze the game immediately
            FreezeGame();
            
            // Get the completion time from timer
            string completionTime = "00:00:00";
            if (Timer.instance != null)
            {
                completionTime = Timer.instance.GetFormattedTime();
                Timer.instance.StopTimer();
            }
            
            // Show game over UI
            if (gameOverUI != null)
            {
                Debug.Log("Showing Game Over UI");
                gameOverUI.ShowGameOver(completionTime);
            }
            else
            {
                Debug.LogError("GameOverUI reference not set in GameManager - trying to find it");
                // Try to find it in the scene if not assigned
                gameOverUI = FindFirstObjectByType<GameOverUI>();
                if (gameOverUI != null)
                {
                    gameOverUI.ShowGameOver(completionTime);
                }
                else
                {
                    Debug.LogError("GameOverUI not found in scene!");
                }
            }
        }
        else
        {
            Debug.Log("GameOver called but ignored - isGameOver: " + isGameOver + ", isSceneTransitioning: " + isSceneTransitioning);
        }
    }
    
    // Freeze the game when player dies
    private void FreezeGame()
    {
        if (!isGameFrozen)
        {
            isGameFrozen = true;
            
            // Freeze time - this will stop all physics and most Update loops
            Time.timeScale = 0f;
            
            // Disable player input
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
                Debug.Log("Player movement disabled on game freeze");
            }
            
            Debug.Log("Game frozen on game over");
        }
    }
    
    // Unfreeze the game when restarting
    private void UnfreezeGame()
    {
        if (isGameFrozen)
        {
            isGameFrozen = false;
            Time.timeScale = 1f;
            Debug.Log("Game unfrozen");
        }
    }
    
    // Reset the game state when starting a new game
    public void RestartGame()
    {
        isGameOver = false;
        isSceneTransitioning = true;
        
        // Make sure to unfreeze the game before loading a new scene
        UnfreezeGame();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    // Add this method to properly handle scene loads
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset state when a new scene is loaded
        isGameOver = false;
        isSceneTransitioning = false;
        
        // Try to find UI references if not set
        if (gameOverUI == null)
        {
            gameOverUI = FindFirstObjectByType<GameOverUI>();
        }
    }
}