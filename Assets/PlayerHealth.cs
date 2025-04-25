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

        // Find UI Manager in the scene
        uiManager = FindObjectOfType<UIManager>();

        // Update UI
        UpdateHealthUI();
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
        StartCoroutine(InvulnerabilityFrames());

        // Check if player is dead
        if (currentHealth <= 0)
        {
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
    }

    void Die()
    {
        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Disable player controls
        playerMovement.enabled = false;

        // Disable collisions
        GetComponent<Collider2D>().enabled = false;

        // Prevent further movement
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        // Play death animation (could be added here)

        // Restart level after delay
        StartCoroutine(RestartLevel(2f));
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