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
    public bool includeBreakableAsGround = true; // Whether to consider breakable objects as ground

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;    
    [Header("Punch Settings")]
    public Transform punchPoint;
    public float punchRange = 0.25f;
    public LayerMask breakableLayer;
    public LayerMask enemyLayer;
    public int punchDamage = 1;
    private bool isPunching = false;
    public float punchCooldown = 0.5f; // Cooldown between punches
    private float lastPunchTime = -Mathf.Infinity; // Track when last punch occurred
    public GameObject punchCooldownFeedback; // Optional: UI element to show cooldown

    [Header("Spit Settings")]
    public Transform spitPoint;
    public GameObject spitPrefab;
    public float spitForce = 8f;
    public float spitCooldown = 3f;
    private float lastSpitTime = -Mathf.Infinity;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip punchSound;
    public AudioClip spitSound;
    public AudioClip landingSound;
    public AudioClip footstepSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    private float footstepTimer = 0f;
    public float footstepInterval = 0.3f;

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


        // Make sure enemyLayer is set to target the Enemy layer if not set in editor
        if (enemyLayer.value == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy"); // Using layer name instead of number
            Debug.LogWarning("Enemy layer was not set, defaulting to Enemy layer");
        }
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

    }

    void Update()
    {
        // Ne pas permettre le mouvement si le niveau est terminé ou si on est en train de frapper
        if (!isLevelCompleted && !isPunching)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
            
            // Gérer le son des pas
            if (Mathf.Abs(horizontalMovement) > 0.1f && Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
            {
                footstepTimer += Time.deltaTime;
                
                // Jouer le son des pas à intervalles réguliers lorsque le joueur se déplace au sol
                if (footstepTimer >= footstepInterval)
                {
                    if (audioSource != null && footstepSound != null)
                    {
                        audioSource.PlayOneShot(footstepSound, soundVolume * 0.7f);
                    }
                    footstepTimer = 0f;
                }
            }
            else
            {
                // Réinitialiser le timer si le joueur ne bouge pas ou n'est pas au sol
                footstepTimer = 0f;
            }
        }
        else if (isLevelCompleted || isPunching)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            footstepTimer = 0f; // Réinitialiser le timer quand le joueur ne peut pas bouger
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
        // Ne pas permettre les actions si le jeu est gelé (timeScale = 0)
        if (Time.timeScale == 0f)
        {
            horizontalMovement = 0;
            return;
        }
        
        if (!isLevelCompleted && !isPunching)
        {
            horizontalMovement = context.ReadValue<Vector2>().x;
        }
    }

    // Méthode pour gérer le saut
    public void Jump(InputAction.CallbackContext context)
    {
        // Ne pas permettre les actions si le jeu est gelé (timeScale = 0)
        if (Time.timeScale == 0f) return;
        
        if (jumpsRemaining > 0 && !isLevelCompleted && !isPunching)
        {
            // Si le joueur est au sol ou en l'air, il peut sauter
            if (context.performed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--;
                if (animator != null) animator.SetTrigger("jump");
                
                // Jouer le son de saut
                if (audioSource != null && jumpSound != null)
                {
                    audioSource.PlayOneShot(jumpSound, soundVolume);
                }
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

        bool wasGrounded = jumpsRemaining == maxJumps;
        
        // Check for ground on ground layer
        bool isGrounded = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer);
        
        // Also check for breakable objects if that option is enabled
        if (includeBreakableAsGround)
        {
            // Add to existing ground check (not replace)
            isGrounded = isGrounded || Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, breakableLayer);
        }

        if (isGrounded)
        {
            // Si le joueur n'était pas au sol avant et qu'il atterrit maintenant, jouer le son d'atterrissage
            if (!wasGrounded && rb.linearVelocity.y < -2f && audioSource != null && landingSound != null)
            {
                audioSource.PlayOneShot(landingSound, soundVolume);
            }
            
            jumpsRemaining = maxJumps;
        }
    }    // Méthode pour gérer le punch
    public void Punch(InputAction.CallbackContext context)
    {
        // Ne pas permettre les actions si le jeu est gelé (timeScale = 0)
        if (Time.timeScale == 0f) return;
        
        if (context.performed && !isLevelCompleted)
        {
            float timeSinceLastPunch = Time.time - lastPunchTime;
            if (!isPunching && timeSinceLastPunch >= punchCooldown)
            {
                StartPunch();
            }
            else if (timeSinceLastPunch < punchCooldown)
            {
                // Optional: Show feedback that punch is on cooldown
                Debug.Log("Punch on cooldown: " + (punchCooldown - timeSinceLastPunch).ToString("F1") + "s remaining");
            }
        }
    }

    // Démarrer l'animation de punch
    private void StartPunch()
    {
        isPunching = true;
        lastPunchTime = Time.time; // Update the last punch time
        if (animator != null) animator.SetTrigger("punch");

        // Si l'animation est désactivée ou n'existe pas, appeler PunchHit directement
        if (animator == null)
        {
            PunchHit();
        }
        else
        {
            // If using animator, end punch state after a set time if animation doesn't call PunchHit()
            StartCoroutine(EndPunchAfterDelay(0.5f));
        }
    }
    
    // Make sure punch state ends even if animation doesn't trigger the callback
    private IEnumerator EndPunchAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // If still punching after the delay, reset the state
        if (isPunching)
        {
            isPunching = false;
        }
    }

    // Cette fonction est appelée par l'animation ou directement depuis StartPunch si pas d'animateur
    public void PunchHit()
    {
        // Jouer le son de punch
        if (audioSource != null && punchSound != null)
        {
            audioSource.PlayOneShot(punchSound, soundVolume);
        }
        
        // Vérifier les objets cassables dans la portée du coup
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(punchPoint.position, punchRange, breakableLayer);

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
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(punchPoint.position, punchRange, enemyLayer);        // Si aucun ennemi n'est trouvé avec le masque, essayer directement avec la couche Enemy
        if (hitEnemies.Length == 0)
        {
            hitEnemies = Physics2D.OverlapCircleAll(punchPoint.position, punchRange, LayerMask.GetMask("Enemy"));
        }
          // Appliquer des dégâts aux ennemis (selon punchDamage)
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamageFromBeat(punchDamage, transform.position);
            }
        }

        isPunching = false;
        horizontalMovement = 0; // Arrêter le mouvement horizontal pendant le coup de poing
    }

    public void Spit(InputAction.CallbackContext context)
    {
        // Ne pas permettre les actions si le jeu est gelé (timeScale = 0)
        if (Time.timeScale == 0f) return;
        
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

        // Jouer le son du crachat
        if (audioSource != null && spitSound != null)
        {
            audioSource.PlayOneShot(spitSound, soundVolume);
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

        // Vérifier les ennemis dans une zone plus large pour le débogage
        Collider2D[] allEntities = Physics2D.OverlapCircleAll(punchPoint.position, punchRange * 3);

        foreach (Collider2D entity in allEntities)
        {
            // Vérifier si l'entité a un composant Enemy
            Enemy enemyComponent = entity.GetComponent<Enemy>();
        }
    }
}