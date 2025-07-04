using UnityEngine;

public class SpitProjectile : MonoBehaviour
{
    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    public int damage = 1;

    private float maxLifetime = 3f;
    private Rigidbody2D rb;
    private bool isSliding = false;
    private float slideSpeed = 0.5f; // Vitesse de glissement réduite
    private float stickyFactor = 0.95f; // Facteur de "collage"
    private bool hasHitEnemy = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, maxLifetime);

        // Si la couche ennemie n'est pas définie, utiliser la couche "Enemy" par défaut
        if (enemyLayer.value == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
        }

        // Si la couche sol n'est pas définie, utiliser le layer 3 (Ground) par défaut
        if (groundLayer.value == 0)
        {
            groundLayer = 1 << 3;
        }
    }

    private void Update()
    {
        // Check for enemies if projectile hasn't hit an enemy yet
        if (!hasHitEnemy)
        {
            CheckForEnemyCollision();
        }
    }

    private void FixedUpdate()
    {
        if (isSliding)
        {
            // Ralentit progressivement le crachat
            rb.linearVelocity *= stickyFactor;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if collision is with ground
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            HandleGroundCollision(collision);
        }

        // Check if collision is with enemy
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            DamageEnemy(collision.gameObject);
        }
    }

    private void HandleGroundCollision(Collision2D collision)
    {
        Vector2 normal = collision.contacts[0].normal;
        float angle = Vector2.Angle(normal, Vector2.up);

        if (angle > 45f)
        {
            // Collision with a wall or steep surface
            isSliding = true;
            rb.gravityScale = 0.1f;
            rb.linearDamping = 5f; // Augmente la résistance
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * slideSpeed, rb.linearVelocity.y * slideSpeed);
        }
        else
        {
            // Collision with a more horizontal surface, destroy after a brief delay
            Destroy(gameObject, 1.5f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if projectile hit the ground
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // Check if projectile hit an enemy
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            // Apply damage to the enemy
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamageFromSpit(damage, transform.position);
                hasHitEnemy = true;
                Destroy(gameObject, 0.1f); // Destroy after a short delay for visual feedback
            }
        }
    }

    private void CheckForEnemyCollision()
    {
        // Create a small overlap circle to detect enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 0.2f, enemyLayer);

        // Damage enemies hit
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            DamageEnemy(enemyCollider.gameObject);
            return; // Only damage one enemy at a time
        }
    }    private void DamageEnemy(GameObject enemy)
    {
        // Prevent hitting multiple enemies with the same projectile
        if (hasHitEnemy) return;

        // Get enemy component and apply freeze effect (no damage)
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            // Send 0 damage to just apply freeze effect
            enemyComponent.TakeDamageFromSpit(0, transform.position);
            hasHitEnemy = true;

            // Destroy the projectile after hitting an enemy (with slight delay for effects)
            Destroy(gameObject, 0.1f);
        }
    }
}