using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour
{
    [Header("End Level Settings")]
    public float delayBeforeNextLevel = 3.0f;
    public string nextLevelName;
    public bool isLastLevel = false;

    [Header("Effects")]
    public AudioClip victorySound;

    private bool levelCompleted = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !levelCompleted)
        {
            CompleteLevel(collision.gameObject);
        }
    }

    void CompleteLevel(GameObject player)
    {
        levelCompleted = true;

        // Play victory sound
        if (victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
        }

        // Get player components
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        Animator playerAnimator = player.GetComponent<Animator>();

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Play "Tbag" animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Tbag");
        }

        // Go to next level after delay
        Invoke("LoadNextLevel", delayBeforeNextLevel);
    }

    void LoadNextLevel()
    {
        if (isLastLevel)
        {
            // Load main menu or show victory screen
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // Load next level
            SceneManager.LoadScene(nextLevelName);
        }
    }
}