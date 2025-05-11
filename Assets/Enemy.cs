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

    [Header("Player Detection")]
    public bool enablePlayerDetection = true;
    public float playerDetectionRadius = 5f;
    public LayerMask playerDetectionLayer;
    public bool playerDetected = false;
    public float chaseSpeedMultiplier = 1.2f;
    public float flipCooldown = 0.5f;  // Cooldown time between direction flips
    private float lastFlipTime = 0f;   // Last time the enemy flipped direction
    public float flipHysteresis = 0.8f;  // Wider zone for flipping decision stability

    [Header("Collision")]
    public bool ignorePlayerCollision = true;
    private Coroutine freezeCoroutine;

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
    private bool canAttack = true; [Header("Effects")]
    public float knockbackForce = 5f;
    public GameObject deathEffect;
    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
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
        if (ignorePlayerCollision)
        {
            // Uncommenting this line would make players walk through enemies
            Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Player"), true);
        }
        // Initialize health
        currentHealth = maxHealth;

        // Set initial movement direction
        movement = faceRight ? Vector2.right : Vector2.left;

        // Ensure this GameObject is on the Enemy layer
        if (LayerMask.LayerToName(gameObject.layer) != "Enemy")
        {
            int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
            if (enemyLayerIndex != -1)
            {
                gameObject.layer = enemyLayerIndex;
            }
        }

        // Initialize health bar display
        InitializeHealthBar();
    }
    void Update()
    {
        // Don't do anything if dead or stunned/frozen
        if (isDead || isStunned) return;

        // Check for player detection
        if (enablePlayerDetection)
        {
            DetectPlayer();
        }

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
            // Only flip if player is not detected
            if (!playerDetected && (!isGrounded || CheckWall()))
            {
                Flip();
            }

            // Move in current direction
            float currentSpeed = playerDetected ? moveSpeed * chaseSpeedMultiplier : moveSpeed;
            rb.linearVelocity = new Vector2(movement.x * currentSpeed, rb.linearVelocity.y);

            // Try to attack if player is in range
            if (canAttack)
            {
                CheckForPlayerInRange();
            }
        }
    }

    bool CheckWall()
    {
        // Direction based on which way the enemy is facing
        float direction = faceRight ? 1f : -1f;
        Vector2 rayDirection = new Vector2(direction, 0);
        float rayDistance = 0.5f;
        
        // Try with layer masks first (more readable approach)
        int groundLayer = LayerMask.GetMask("Ground", "Breakable");
        int enemyLayer = LayerMask.GetMask("Enemy");
        
        // Fallback layer numbers in case names aren't set properly
        if (groundLayer == 0) groundLayer = (1 << 3) | (1 << 6);
        if (enemyLayer == 0) enemyLayer = 1 << 8;
        
        // Check for walls/obstacles
        bool hitWall = Physics2D.Raycast(transform.position, rayDirection, rayDistance, groundLayer).collider != null;
        
        // Check for other enemies
        bool hitEnemy = Physics2D.Raycast(transform.position, rayDirection, rayDistance, enemyLayer).collider != null;
        
        return hitWall || hitEnemy;
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

        currentHealth -= damage;        // Apply knockback
        Vector2 knockbackDir = ((Vector2)transform.position - source).normalized;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        // Visual feedback
        StartCoroutine(FlashColor());

        // Stun the enemy briefly
        StartCoroutine(StunEnemy(0.5f));

        // Update health display
        UpdateHealthDisplay();

        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }    // Apply freeze effect from spit projectile (no damage)
    public void TakeDamageFromSpit(int damage, Vector2 source)
    {
        // Don't modify health, just apply freeze
        if (isDead) return;

        // Cancel any existing stun/freeze coroutines if we have one running
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
            freezeCoroutine = null;
        }

        // Cancel any regular stun coroutines too to avoid conflicts
        StopAllCoroutines();

        // Set color back to normal briefly for visual feedback
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        // Apply freeze effect that completely stops the enemy
        isStunned = true;

        // Apply new freeze effect (restarts the timer if already frozen)
        freezeCoroutine = StartCoroutine(FreezeEffect(1.7f));
    }

    // Special coroutine for freezing effect from spit
    IEnumerator FreezeEffect(float freezeDuration)
    {
        // Store original state to restore later
        RigidbodyType2D originalType = rb.bodyType;
        Vector2 originalVelocity = rb.linearVelocity;
        // Freeze the enemy movement
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // Prevent physics forces

        // Apply visual freeze effect
        StartCoroutine(IceColorEffect());

        // Wait for the entire freeze duration
        yield return new WaitForSeconds(freezeDuration);

        // Unfreeze only if not dead
        if (!isDead)
        {
            // Reset renderer color
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;

            // Restore physics
            rb.bodyType = originalType;
            isStunned = false;
        }

        freezeCoroutine = null;
    }

    // Visual effect for freeze
    IEnumerator IceColorEffect()
    {
        if (spriteRenderer == null) yield break;

        // Create a pulsing ice effect
        float duration = 0.4f;

        // Initial flash
        spriteRenderer.color = new Color(0.7f, 0.9f, 1f); // Ice blue
        yield return new WaitForSeconds(duration);

        spriteRenderer.color = new Color(0.8f, 0.95f, 1f); // Lighter blue
        yield return new WaitForSeconds(duration);
        // Keep pulsing between two shades of ice blue while frozen
        while (isStunned && !isDead)
        {
            // Darker ice blue
            spriteRenderer.color = new Color(0.6f, 0.8f, 1f);
            yield return new WaitForSeconds(duration);

            // Lighter ice blue
            spriteRenderer.color = new Color(0.8f, 0.95f, 1f);
            yield return new WaitForSeconds(duration);
        }

        // Reset color if coroutine exits but enemy isn't dead (might have been unfrozen)
        if (!isDead && spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    // Apply damage specifically from player's beat/punch attack
    public void TakeDamageFromBeat(int damage, Vector2 source)
    {
        if (isDead) return;

        // Apply increased damage for beat attacks
        int beatDamage = damage; // Double damage for beat attacks

        // Apply special knockback effects
        Vector2 knockbackDir = ((Vector2)transform.position - source).normalized;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);        // Reduce health
        currentHealth -= beatDamage;

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

        // Stop movement
        rb.linearVelocity = Vector2.zero;

        // Stop all coroutines to prevent visual effects from continuing
        StopAllCoroutines();

        // Disable sprite renderer immediately to make enemy invisible
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }        // Disable collider
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Destroy after delay (for sound and effects to complete)
        Destroy(gameObject, 1f);
    }

    void Flip()
    {
        // Prevent flipping too frequently
        if (Time.time - lastFlipTime < flipCooldown) return;

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

        // Update last flip time
        lastFlipTime = Time.time;
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

        // Draw player detection radius
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
    }

    // Initialise la barre de vie avec des sprites de cœur
    void InitializeHealthBar()
    {
        // Si le prefab de cœur n'est pas assigné
        if (heartPrefab == null)
        {
            return;
        }

        // Crée un parent pour les cœurs si nécessaire
        if (healthBarParent == null)
        {
            GameObject healthBarObj = new GameObject("EnemyLife");
            healthBarParent = healthBarObj.transform;
            healthBarParent.SetParent(transform);
            healthBarParent.localPosition = healthBarOffset;
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
            }

            // S'assurer que le cœur est visible (ordre de tri et layer)
            spriteRenderer.sortingOrder = 10; // Mettre en premier plan

            // Positionner le cœur correctement
            heart.transform.localPosition = new Vector3(i * heartSpacing - ((maxHealth - 1) * heartSpacing / 2), 0, 0);
            heart.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // Adapter la taille si nécessaire
            heart.SetActive(true);
            healthHearts.Add(heart);
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
    }    void DetectPlayer()
    {
        // Check for player within detection radius
        Collider2D player = Physics2D.OverlapCircle(transform.position, playerDetectionRadius, playerDetectionLayer);

        // If player layer mask is not set, try with default player layer (usually layer 7)
        if (player == null && playerDetectionLayer == 0)
        {
            player = Physics2D.OverlapCircle(transform.position, playerDetectionRadius, 1 << 7);
        }

        // Store previous detection state
        bool wasDetected = playerDetected;
        
        // Update detection status
        playerDetected = player != null;

        if (playerDetected && player != null)
        {
            // Determine if player is to the left or right of enemy
            float horizontalDistance = player.transform.position.x - transform.position.x;
            bool playerIsRight = horizontalDistance > 0;
            
            // Larger deadzone for starting to flip, smaller for continuing in same direction
            float flipDeadZone = wasDetected ? flipHysteresis * 0.7f : flipHysteresis;
            
            // Only flip and change direction if the horizontal distance is significant enough
            if (Mathf.Abs(horizontalDistance) > flipDeadZone)
            {
                // If enemy is not facing the player, turn to face them
                // But respect the cooldown timer to prevent constant flipping
                if ((playerIsRight && !faceRight) || (!playerIsRight && faceRight))
                {
                    if (Time.time - lastFlipTime >= flipCooldown)
                    {
                        Flip();
                    }
                }

                // Set movement direction toward player, but only if we can move or the direction has changed
                movement = new Vector2(playerIsRight ? 1 : -1, rb.linearVelocity.y);
            }
            else
            {
                // When player is directly above/below or very close, slow down horizontal movement
                // This creates a more natural "slowing down" when approaching the player
                if (movement.x != 0)
                {
                    // Gradually slow down rather than immediately stop
                    movement = new Vector2(movement.x * 0.5f, rb.linearVelocity.y);
                    
                    // If moving very slow, just stop
                    if (Mathf.Abs(movement.x) < 0.1f)
                    {
                        movement = new Vector2(0, rb.linearVelocity.y);
                    }
                }
            }
        }
    }
}