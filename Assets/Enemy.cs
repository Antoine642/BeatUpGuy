using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public bool canMove = true;
    public bool faceRight = true;
    public Transform groundDetection;
    public float detectionDistance = 0.5f;

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

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

    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
    }

    void Update()
    {
        if (isDead || isStunned) return;

        if (canMove)
        {
            // Check for edges or walls
            RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, detectionDistance);

            // If no ground detected or hit a wall, turn around
            if (!groundInfo.collider || CheckWall())
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
        return wallInfo.collider != null;
    }

    void CheckForPlayerInRange()
    {
        // Check if player is within attack range
        Collider2D player = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);

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

        // Animation could be triggered here

        // Wait for cooldown
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    public void TakeDamage(int damage, Vector2 source)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Apply knockback
        Vector2 knockbackDir = ((Vector2)transform.position - source).normalized;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        // Play hit sound
        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Visual feedback
        StartCoroutine(FlashColor());

        // Stun the enemy briefly
        StartCoroutine(StunEnemy(0.5f));

        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Apply damage specifically from spit projectile
    public void TakeDamageFromSpit(int damage, Vector2 source)
    {
        // Apply reduced damage for spit
        int spitDamage = Mathf.Max(1, damage / 2);
        TakeDamage(spitDamage, source);

        // Add additional stun time for spit
        StartCoroutine(StunEnemy(1.5f));
    }

    IEnumerator StunEnemy(float stunTime)
    {
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }

    IEnumerator FlashColor()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void Die()
    {
        isDead = true;

        // Stop movement
        rb.linearVelocity = Vector2.zero;

        // Disable collider
        GetComponent<Collider2D>().enabled = false;

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
        transform.Rotate(0, 180, 0);
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

        // Draw ground detection ray
        if (groundDetection != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(groundDetection.position, groundDetection.position + Vector3.down * detectionDistance);
        }
    }
}