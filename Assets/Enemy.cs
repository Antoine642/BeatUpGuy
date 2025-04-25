using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Ajout pour List

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public bool canMove = true;
    public bool faceRight = true;
    public Transform groundDetection;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public float detectionDistance = 0.5f;

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;
    public GameObject heartPrefab; // Prefab pour les cœurs
    public Transform healthBarParent; // Parent pour l'affichage des cœurs
    public float heartSpacing = 0.1f; // Espacement entre les cœurs
    public Vector3 healthBarOffset = new Vector3(0, 1f, 0); // Position au-dessus de l'ennemi
    private List<GameObject> healthHearts = new List<GameObject>(); // Liste des cœurs affichés

    [Header("Attack")]
    public int damage = 1;
    public float attackRange = 0.5f;
    public LayerMask playerLayer;
    public Transform attackPoint;
    public float attackCooldown = 1.5f;
    private bool canAttack = true;

    [Header("Effects")]
    public float knockbackForce = 5f;
    public GameObject deathEffect;
    public AudioClip hitSound;
    public AudioClip deathSound;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isDead = false;
    private bool isStunned = false;
    private Vector2 movement;
    private BoxCollider2D boxCollider;

    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Make sure box collider exists
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(0.5f, 0.8f);
        }

        // Initialize audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initialize health
        currentHealth = maxHealth;

        // Set initial movement direction
        movement = faceRight ? Vector2.right : Vector2.left;

        // Log enemy initialization for debugging
        Debug.Log(gameObject.name + " initialized on layer " + gameObject.layer + " with tag " + gameObject.tag);

        // Ensure this GameObject is on layer 8 (Enemy layer)
        if (gameObject.layer != 8)
        {
            Debug.LogWarning(gameObject.name + " is not on the Enemy layer (8). Current layer: " + gameObject.layer);
        }

        // Initialize health bar display
        InitializeHealthBar();
        Debug.Log("Health bar initialized with " + maxHealth + " hearts");
    }

    void Update()
    {
        if (isDead || isStunned) return;

        if (canMove)
        {
            // Check for ground using overlap box method
            bool isGrounded = false;
            if (groundDetection != null)
            {
                isGrounded = Physics2D.OverlapBox(groundDetection.position, groundCheckSize, 0, LayerMask.GetMask("Ground"));

                // Additional check in case "Ground" is layer 3 but not named "Ground"
                if (!isGrounded)
                {
                    isGrounded = Physics2D.OverlapBox(groundDetection.position, groundCheckSize, 0, 1 << 3);
                }
            }
            else
            {
                // Fallback if groundDetection is missing
                isGrounded = Physics2D.Raycast(transform.position + new Vector3(0, -0.5f, 0), Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
            }

            // If no ground detected or hit a wall, flip the character
            if (!isGrounded || CheckWall())
            {
                Flip();
            }

            // Move in current direction
            rb.linearVelocity = new Vector2(movement.x * moveSpeed, rb.linearVelocity.y);

            // Try to attack if player is in range
            if (canAttack)
            {
                CheckForPlayerInRange();
            }
        }
    }

    bool CheckWall()
    {
        // Check if there's a wall in front of the enemy
        float direction = faceRight ? 1f : -1f;
        RaycastHit2D wallInfo = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 0.5f, LayerMask.GetMask("Ground"));

        // Additional check with layer number in case layer name is not correctly set
        if (wallInfo.collider == null)
        {
            wallInfo = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 0.5f, 1 << 3);
        }

        return wallInfo.collider != null;
    }

    void CheckForPlayerInRange()
    {
        // Check if player is within attack range
        Collider2D player = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);

        if (player == null)
        {
            // Also try with layer number in case mask is not set correctly
            player = Physics2D.OverlapCircle(attackPoint.position, attackRange, 1 << 7); // Player is typically on layer 7
        }

        if (player != null)
        {
            StartCoroutine(Attack(player));
        }
    }

    IEnumerator Attack(Collider2D player)
    {
        canAttack = false;

        // Deal damage to player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage, transform.position);
        }

        // Wait for cooldown
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    public void TakeDamage(int damage, Vector2 source)
    {
        if (isDead) return;

        Debug.Log(gameObject.name + " taking " + damage + " damage");

        currentHealth -= damage;

        // Apply knockback
        Vector2 knockbackDir = ((Vector2)transform.position - source).normalized;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Visual feedback
        StartCoroutine(FlashColor());

        // Stun the enemy briefly
        StartCoroutine(StunEnemy(0.5f));

        // Update health display
        UpdateHealthDisplay();
        Debug.Log("Updated health display. Current health: " + currentHealth);

        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Apply damage specifically from spit projectile
    public void TakeDamageFromSpit(int damage, Vector2 source)
    {
        Debug.Log(gameObject.name + " hit by spit - stunning only, no damage");

        // Don't apply damage, just stun the enemy
        Vector2 knockbackDir = ((Vector2)transform.position - source).normalized;
        
        // Visual feedback
        StartCoroutine(FlashColor());

        // Apply stun effect
        StartCoroutine(StunEnemy(2.0f));
    }

    // Apply damage specifically from player's beat/punch attack
    public void TakeDamageFromBeat(int damage, Vector2 source)
    {
        if (isDead) return;

        Debug.Log(gameObject.name + " taking beat damage: " + damage);

        // Apply increased damage for beat attacks
        int beatDamage = damage; // Double damage for beat attacks

        // Apply special knockback effects
        Vector2 knockbackDir = ((Vector2)transform.position - source).normalized;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        // Reduce health
        currentHealth -= beatDamage;

        // Play hit sound with higher volume
        if (hitSound != null && audioSource != null)
        {
            audioSource.volume = 1.5f;
            audioSource.PlayOneShot(hitSound);
            audioSource.volume = 1.0f; // Reset volume after playing
        }

        // Visual feedback with more intense flashing
        StartCoroutine(BeatFlashEffect());

        // Apply longer stun
        StartCoroutine(StunEnemy(1.2f));

        // Update health display
        UpdateHealthDisplay();

        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Special color flash effect for beat damage
    IEnumerator BeatFlashEffect()
    {
        if (spriteRenderer == null) yield break;

        // Flash red-yellow-red for beat attacks
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    IEnumerator StunEnemy(float stunTime)
    {
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }

    IEnumerator FlashColor()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void Die()
    {
        isDead = true;

        Debug.Log(gameObject.name + " died");

        // Stop movement
        rb.linearVelocity = Vector2.zero;

        // Disable collider
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Destroy after delay
        Destroy(gameObject, 1f);
    }

    void Flip()
    {
        // Flip the sprite and movement direction
        faceRight = !faceRight;

        // Use the same logic as PlayerMovement.FlipSprite
        if (faceRight)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        // Update movement direction
        movement = new Vector2(-movement.x, rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        // Draw attack range
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        // Draw ground detection box
        if (groundDetection != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(groundDetection.position, groundCheckSize);
        }
    }

    // Initialise la barre de vie avec des sprites de cœur
    void InitializeHealthBar()
    {
        // Si le prefab de cœur n'est pas assigné
        if (heartPrefab == null)
        {
            Debug.LogError("Heart prefab is not assigned for " + gameObject.name + ". Please assign a heart sprite prefab in the inspector.");
            return;
        }

        // Crée un parent pour les cœurs si nécessaire
        if (healthBarParent == null)
        {
            GameObject healthBarObj = new GameObject("EnemyLife");
            healthBarParent = healthBarObj.transform;
            healthBarParent.SetParent(transform);
            healthBarParent.localPosition = healthBarOffset;
            Debug.Log("Created new healthBarParent at position " + healthBarOffset);
        }

        // Supprime les anciens cœurs s'il y en a
        foreach (GameObject heart in healthHearts)
        {
            if (heart != null)
                Destroy(heart);
        }
        healthHearts.Clear();

        // Crée les nouveaux cœurs
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, healthBarParent);
            // Assurer que le sprite est visible
            SpriteRenderer spriteRenderer = heart.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = heart.AddComponent<SpriteRenderer>();
                Debug.LogWarning("Heart prefab doesn't have a SpriteRenderer component. Added one automatically.");
            }
            
            // S'assurer que le cœur est visible (ordre de tri et layer)
            spriteRenderer.sortingOrder = 10; // Mettre en premier plan
            
            // Positionner le cœur correctement
            heart.transform.localPosition = new Vector3(i * heartSpacing - ((maxHealth - 1) * heartSpacing / 2), 0, 0);
            heart.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // Adapter la taille si nécessaire
            heart.SetActive(true);
            healthHearts.Add(heart);
            
            Debug.Log("Created heart " + i + " at position " + heart.transform.position);
        }
    }

    // Met à jour l'affichage des points de vie
    void UpdateHealthDisplay()
    {
        // Si la liste des cœurs est vide ou mal configurée
        if (healthHearts.Count == 0 || healthHearts.Count < maxHealth)
        {
            InitializeHealthBar();
            return;
        }

        // Désactive les cœurs en trop en fonction des vies perdues
        for (int i = 0; i < maxHealth; i++)
        {
            if (i < currentHealth)
                healthHearts[i].SetActive(true);
            else
                healthHearts[i].SetActive(false);
        }
    }
}