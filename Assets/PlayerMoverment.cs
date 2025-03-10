using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoverment : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float jumpForce = 10f; // Augmentation de la force de saut
    private bool isGrounded = true; // Vérification si le joueur est au sol
    float horizontalMovement;
    public Animator animator;

    // Start is called une fois avant la première exécution de Update après la création du MonoBehaviour
    void Start()
    {
        
    }

    // Update est appelé une fois par frame
    void Update()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);

        // Vérification si le joueur appuie sur la touche Espace et s'il est au sol
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false; // Le joueur n'est plus au sol après avoir sauté
        }

        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    // Méthode pour détecter la collision avec le sol
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // Le joueur est de nouveau au sol
        }
    }
}