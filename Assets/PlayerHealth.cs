using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Invulnerability")]
    public float invulnerabilityTime = 1.5f;
    public float knockbackForce = 7f;
    private bool isInvulnerable = false;

    [Header("Effects")]
    public AudioClip hitSound;
    public AudioClip deathSound;
    public GameObject hitEffect;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;
    private AudioSource audioSource;

    // UI Reference (could be connected to a UI manager)
    private UIManager uiManager;    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();

        // Initialize audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set initial health
        currentHealth = maxHealth;
        
        // S'assurer que l'état d'invulnérabilité est réinitialisé au démarrage
        isInvulnerable = false;

        // Find UI Manager in the scene
        uiManager = FindFirstObjectByType<UIManager>();

        // Update UI
        UpdateHealthUI();
        
        Debug.Log("PlayerHealth initialized - Health: " + currentHealth + ", Invulnerable: " + isInvulnerable);
    }

    public void TakeDamage(int damage, Vector2 damageSource)
    {
        // Check if player can take damage
        if (isInvulnerable) return;

        // Reduce health
        currentHealth -= damage;

        // Update UI
        UpdateHealthUI();

        // Play hit sound
        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Spawn hit effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Apply knockback
        Vector2 knockbackDirection = ((Vector2)transform.position - damageSource).normalized;
        rb.linearVelocity = Vector2.zero; // Reset velocity before applying knockback
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // Start invulnerability
        StartCoroutine(InvulnerabilityFrames());        // Check if player is dead
        if (currentHealth <= 0)
        {
            Debug.Log("Player health is " + currentHealth + ", calling Die()");
            Die();
        }
    }

    IEnumerator InvulnerabilityFrames()
    {
        isInvulnerable = true;

        // Visual feedback for invulnerability (flashing)
        float flashDuration = invulnerabilityTime;
        float elapsed = 0;

        while (elapsed < flashDuration)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);

            elapsed += 0.2f;
        }

        // Return to normal state
        spriteRenderer.color = Color.white;
        isInvulnerable = false;
    }    void Die()
    {
        // Check if we're already dead to avoid multiple calls
        if (currentHealth > 0) return;
        
        Debug.Log("Die function executing...");
        
        // Make player invulnerable but don't return if already invulnerable
        // This ensures death process continues even during invulnerability frames
        isInvulnerable = true;
        
        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Disable player controls
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Disable collisions
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Prevent further movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }

        // Play death animation (could be added here)

        // Signal game over to GameManager
        if (GameManager.instance != null)
        {
            Debug.Log("Calling GameManager.GameOver()");
            // Small delay to ensure UI is ready
            StartCoroutine(DelayedGameOver());
        }
        else
        {
            Debug.LogError("GameManager.instance is null! Trying to find GameManager in scene.");
            // Try to find the GameManager in the scene
            GameManager manager = FindFirstObjectByType<GameManager>();
            if (manager != null)
            {
                Debug.Log("Found GameManager, calling GameOver()");
                // Use the found manager to call GameOver
                manager.GameOver();
            }
            else
            {
                // If GameManager is still not available, use the old restart method
                Debug.LogError("No GameManager found in scene!");
                StartCoroutine(RestartLevel(2f));
            }
        }
    }    private IEnumerator DelayedGameOver()
    {
        // Small delay to ensure everything is ready
        yield return new WaitForSeconds(0.1f);
        Debug.Log("About to call GameManager.GameOver(), GameManager exists: " + (GameManager.instance != null));
        if (GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }
        else
        {
            Debug.LogError("GameManager.instance is null in DelayedGameOver!");
        }
    }

    IEnumerator RestartLevel(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateHealthUI()
    {
        // Update UI if UI Manager exists
        if (uiManager != null)
        {
            uiManager.UpdateHealthDisplay(currentHealth, maxHealth);
        }
    }

    // Method to heal the player
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    // Method to get current health (useful for other scripts)
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}