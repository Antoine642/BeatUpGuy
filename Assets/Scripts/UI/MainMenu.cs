using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject rulesPanel;

    [Header("Buttons")]
    public Button playButton;
    public Button rulesButton;
    public Button backButton;

    [Header("Settings")]
    public string gameSceneName = "Game"; // The name of your game scene
    public Color buttonTextColor = Color.white;
    public Color buttonBackgroundColor = new Color(0.3f, 0.3f, 0.8f);
    public Color buttonHoverColor = new Color(0.4f, 0.4f, 0.9f);

    [Header("Menu Background")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.2f);

    void Start()
    {
        // Make sure we have the necessary panels and buttons
        if (mainMenuPanel == null || rulesPanel == null)
        {
            Debug.LogError("Main menu or rules panel is missing!");
            return;
        }

        // Set initial state
        ShowMainMenu();
    }

    public void PlayGame()
    {
        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowRules()
    {
        // Hide main menu and show rules
        mainMenuPanel.SetActive(false);
        rulesPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        // Hide rules and show main menu
        rulesPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // This method can be called from editor to set up the UI dynamically
    public void SetupUI()
    {
        // This method can be extended to create the UI elements programmatically
        // if they don't exist in the scene already
        Debug.Log("Setting up Main Menu UI...");
    }
}