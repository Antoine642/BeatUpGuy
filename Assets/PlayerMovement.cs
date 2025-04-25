using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;

    [Header("Jump")]
    public float jumpForce = 10f;
    public int maxJumps = 2;
    int jumpsRemaining;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    [Header("Punch Settings")]
    public Transform punchPoint;
    public float punchRange = 0.15f;
    public LayerMask breakableLayer;
    public LayerMask enemyLayer;
    public int punchDamage = 1;
    private bool isPunching = false;

    [Header("Spit Settings")]
    public Transform spitPoint;
    public GameObject spitPrefab;
    public float spitForce = 8f;
    public float spitCooldown = 3f;
    private float lastSpitTime = -Mathf.Infinity;

    [Header("Level End")]
    private bool isLevelCompleted = false;

    [Header("Animator")]
    public Animator animator;

    void Start()
    {
        // Initialize component references
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();

        // Validate critical components
        if (groundCheckPos == null)
            Debug.LogError("GroundCheckPos is missing on PlayerMovement!");
        if (punchPoint == null)
            Debug.LogError("PunchPoint is missing on PlayerMovement!");

        // Validate layers
        Debug.Log("Enemy Layer value: " + enemyLayer.value);
        Debug.Log("Player is on layer: " + gameObject.layer);

        // Make sure enemyLayer is set to target layer 8 if not set in editor
        if (enemyLayer.value == 0)
        {
            enemyLayer = 1 << 8; // Set to layer 8 (Enemy layer)
            Debug.LogWarning("Enemy layer was not set, defaulting to layer 8");
        }
    }

    void Update()
    {
        // Ne pas permettre le mouvement si le niveau est terminé ou si on est en train de frapper
        if (!isLevelCompleted && !isPunching)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
        }
        else if (isLevelCompleted || isPunching)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (animator != null)
        {
            animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }

        GroundCheck();
        Gravity();

        // Flip the player sprite based on horizontal movement
        FlipSprite();
    }

    // Method to flip the sprite based on movement direction
    private void FlipSprite()
    {
        if (horizontalMovement != 0)
        {
            // If moving right, ensure no rotation
            if (horizontalMovement > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            // If moving left, rotate to face left
            else if (horizontalMovement < 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    // Méthode pour gérer la gravité
    private void Gravity()
    {
        // Si le joueur est en l'air, on applique la gravité
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier; // Applique le multiplicateur de gravité
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -maxFallSpeed, Mathf.Infinity)); // On limite la vitesse de chute maximale
        }
        // Sinon on applique la gravité de base
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    // Méthode pour gérer le mouvement horizontal
    public void Move(InputAction.CallbackContext context)
    {
        if (!isLevelCompleted && !isPunching)
        {
            horizontalMovement = context.ReadValue<Vector2>().x;
        }
    }

    // Méthode pour gérer le saut
    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0 && !isLevelCompleted && !isPunching)
        {
            // Si le joueur est au sol ou en l'air, il peut sauter
            if (context.performed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--;
                if (animator != null) animator.SetTrigger("jump");
            }
            // si le joueur est en l'air et que le saut est annulé, il peut sauter à nouveau
            else if (context.canceled && !Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                jumpsRemaining--;
                if (animator != null) animator.SetTrigger("jump");
            }
        }
    }

    // Méthode pour vérifier si le joueur est au sol
    private void GroundCheck()
    {
        if (groundCheckPos == null) return;

        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
        }
    }

    // Méthode pour gérer le punch
    public void Punch(InputAction.CallbackContext context)
    {
        if (context.performed && !isPunching && !isLevelCompleted)
        {
            StartPunch();
        }
    }

    // Démarrer l'animation de punch
    private void StartPunch()
    {
        isPunching = true;
        if (animator != null) animator.SetTrigger("punch");

        // Si l'animation est désactivée ou n'existe pas, appeler PunchHit directement
        if (animator == null)
        {
            PunchHit();
        }
    }

    // Cette fonction est appelée par l'animation ou directement depuis StartPunch si pas d'animateur
    public void PunchHit()
    {
        // Debug
        Debug.Log("PunchHit called. Punch position: " + punchPoint.position + ", range: " + punchRange);

        // Vérifier les objets cassables dans la portée du coup
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(punchPoint.position, punchRange, breakableLayer);
        Debug.Log("Found " + hitObjects.Length + " breakable objects");

        // Appliquer des dégâts aux objets cassables
        foreach (Collider2D obj in hitObjects)
        {
            Breakable breakable = obj.GetComponent<Breakable>();
            if (breakable != null)
            {
                breakable.TakeHit();
            }
        }

        // Vérifier les ennemis dans la portée du coup
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(punchPoint.position, punchRange, enemyLayer);
        Debug.Log("Found " + hitEnemies.Length + " enemies within punch range using layer mask: " + enemyLayer.value);

        // Si aucun ennemi n'est trouvé avec le masque, essayer directement avec la couche 8
        if (hitEnemies.Length == 0)
        {
            hitEnemies = Physics2D.OverlapCircleAll(punchPoint.position, punchRange, 1 << 8);
            Debug.Log("Retry with layer 8 found " + hitEnemies.Length + " enemies");
        }

        // Appliquer des dégâts aux ennemis
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("Applying damage to enemy: " + enemy.name);
                enemy.TakeDamageFromBeat(punchDamage, transform.position);
            }
            else
            {
                Debug.Log("Found collider but no Enemy component on " + enemyCollider.name);
            }
        }

        isPunching = false;
        horizontalMovement = 0; // Arrêter le mouvement horizontal pendant le coup de poing
    }

    public void Spit(InputAction.CallbackContext context)
    {
        if (context.performed && !isLevelCompleted && !isPunching)
        {
            StartSpit();
        }
    }

    // Démarrer l'animation de crachat
    private void StartSpit()
    {
        if (Time.time - lastSpitTime < spitCooldown) return; // Vérifie le délai de récupération
        if (spitPrefab == null)
        {
            Debug.LogError("Spit prefab is missing!");
            return;
        }

        lastSpitTime = Time.time; // Met à jour le temps du dernier crachat
        GameObject spit = Instantiate(spitPrefab, spitPoint.position, spitPoint.rotation);
        Rigidbody2D spitRb = spit.GetComponent<Rigidbody2D>();

        if (spitRb != null)
        {
            spitRb.AddForce(spitPoint.right * spitForce, ForceMode2D.Impulse);
        }

        // Configurer le script SpitProjectile
        SpitProjectile spitProjectile = spit.GetComponent<SpitProjectile>();
        if (spitProjectile != null)
        {
            spitProjectile.groundLayer = groundLayer;
            spitProjectile.enemyLayer = enemyLayer;
        }
    }

    // Méthode pour déclencher l'animation Tbag (appelée par LevelEnd.cs)
    public void TriggerTbagAnimation()
    {
        isLevelCompleted = true;
        horizontalMovement = 0;
        if (animator != null) animator.SetTrigger("Tbag");
    }

    // Méthode pour détecter la collision avec le sol
    private void OnDrawGizmosSelected()
    {
        // Draw ground check area
        if (groundCheckPos != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        }

        // Draw punch range
        if (punchPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(punchPoint.position, punchRange);
        }
    }

    // Pour le débogage du coup de poing
    public void OnEnemyHit(int damage)
    {
        // Log détaillé des conditions de recherche d'ennemis
        Debug.Log("OnEnemyHit called with damage: " + damage);
        Debug.Log("Layer Enemy mask: " + enemyLayer.value);
        Debug.Log("Punch range: " + punchRange);
        Debug.Log("Punch position: " + punchPoint.position);

        // Vérifier les ennemis dans une zone plus large pour le débogage
        Collider2D[] allEntities = Physics2D.OverlapCircleAll(punchPoint.position, punchRange * 3);
        Debug.Log("Found " + allEntities.Length + " entities in extended range");

        foreach (Collider2D entity in allEntities)
        {
            Debug.Log("Entity nearby: " + entity.name + " Layer: " + LayerMask.LayerToName(entity.gameObject.layer) + " (Layer #" + entity.gameObject.layer + ")");

            // Vérifier si l'entité a un composant Enemy
            Enemy enemyComponent = entity.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                Debug.Log("Entity has Enemy component");
            }
        }
    }
}