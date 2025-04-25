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
    }

    private void Update()
    {
        // Check for enemies if projectile hasn't hit an enemy yet
        if (!hasHitEnemy)
        {
            CheckForEnemyCollision();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if collision is with ground
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Vector2 normal = collision.contacts[0].normal;
            float angle = Vector2.Angle(normal, Vector2.up);

            if (angle > 45f)
            {
                isSliding = true;
                rb.gravityScale = 0.1f;
                rb.linearDamping = 5f; // Augmente la résistance
                rb.linearVelocity = new Vector2(rb.linearVelocity.x * slideSpeed, rb.linearVelocity.y * slideSpeed);
            }
            else
            {
                Destroy(gameObject, 1.5f);
            }
        }

        // Check if collision is with enemy
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            DamageEnemy(collision.gameObject);
        }
    }

    private void CheckForEnemyCollision()
    {
        // Create a small overlap circle to detect enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 0.1f, enemyLayer);

        // Damage enemies hit
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            DamageEnemy(enemyCollider.gameObject);
        }
    }

    private void DamageEnemy(GameObject enemy)
    {
        // Prevent hitting multiple enemies with the same projectile
        if (hasHitEnemy) return;

        // Get enemy component and apply "spit" damage
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.TakeDamageFromSpit(damage, transform.position);
            hasHitEnemy = true;

            // Destroy the projectile after hitting an enemy (with slight delay for effects)
            Destroy(gameObject, 0.1f);
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

    void OnDrawGizmosSelected()
    {
        // Draw enemy detection radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}