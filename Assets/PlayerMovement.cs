using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Movement")]
    public float moveSpeed = 5f; // Vitesse de déplacement
    float horizontalMovement;

    [Header("Jump")]
    public float jumpForce = 10f; // Augmentation de la force de saut
    public int maxJumps = 2; // Nombre de saut maximum
    int jumpsRemaining; // Nombre de saut restant

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f); // Taille de la zone de vérification du sol
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f; // Gravité de base
    public float maxFallSpeed = 18f; // Vitesse de chute maximale
    public float fallSpeedMultiplier = 2f; // Multiplicateur de vitesse de chute

    [Header("Punch Settings")]
    public Transform punchPoint;
    public float punchRange = 1.0f;
    public LayerMask breakableLayer;
    private bool isPunching = false;

    [Header("Level End")]
    private bool isLevelCompleted = false;
    
    [Header("Animator")]
    public Animator animator;

    // Update est appelé une fois par frame
    void Update()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        GroundCheck();
        Gravity();
    }
    // Méthode pour gérer la gravité
    private void Gravity()
    {
        // Si le joueur est en l'air, on applique la gravité
        if(rb.linearVelocity.y < 0)
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
        horizontalMovement = context.ReadValue<Vector2>().x;
    }
    // Méthode pour gérer le saut
    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0)
        {
            // Si le joueur est au sol ou en l'air, il peut sauter
            if (context.performed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--;
                animator.SetTrigger("jump"); // Applique le trigger de saut
            }
            // si le joueur est en l'air et que le saut est annulé, il peut sauter à nouveau
            else if (context.canceled && !Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                jumpsRemaining--;
                animator.SetTrigger("jump"); // Applique le trigger de saut
            }
        }
    }
    // Méthode pour vérifier si le joueur est au sol
    private void GroundCheck()
    {
        if(Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
            animator.ResetTrigger("jump"); // Reset le trigger de saut lorsque le joueur touche le sol
        }
    }

    // Méthode pour détecter la collision avec le sol
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    }
}