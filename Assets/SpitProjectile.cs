using UnityEngine;

public class SpitProjectile : MonoBehaviour
{
    public LayerMask groundLayer;
    private float maxLifetime = 3f;
    private Rigidbody2D rb;
    private bool isSliding = false;
    private float slideSpeed = 0.5f; // Vitesse de glissement réduite
    private float stickyFactor = 0.95f; // Facteur de "collage"

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, maxLifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
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
    }

    private void FixedUpdate()
    {
        if (isSliding)
        {
            // Ralentit progressivement le crachat
            rb.linearVelocity *= stickyFactor;
        }
    }
}