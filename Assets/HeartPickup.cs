using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    [Header("Properties")]
    public int healthRestoreAmount = 1;
    public float attractRange = 4f;
    public float attractSpeed = 5f;
    public float bobHeight = 0.3f;
    public float bobSpeed = 2f;
    
    [Header("Effects")]
    public AudioClip pickupSound;
    public GameObject pickupEffect;
    
    private Transform player;
    private Vector3 startPosition;
    private float bobTimer;
    
    void Start()
    {
        startPosition = transform.position;
        bobTimer = Random.Range(0f, 2f * Mathf.PI); // Random start point in the cycle
        
        // Find player in scene
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void Update()
    {
        // Bob up and down
        bobTimer += Time.deltaTime * bobSpeed;
        Vector3 bobPosition = startPosition;
        bobPosition.y += Mathf.Sin(bobTimer) * bobHeight;
        transform.position = bobPosition;
        
        // Attract to player when close
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance < attractRange)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    player.position, 
                    attractSpeed * Time.deltaTime);
                
                // Increase attraction speed as we get closer
                attractSpeed = Mathf.Lerp(attractSpeed, attractSpeed * 2, Time.deltaTime);
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Get player health component
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                // Heal the player
                playerHealth.Heal(healthRestoreAmount);
                
                // Play pickup sound
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                
                // Spawn pickup effect
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                // Destroy the heart
                Destroy(gameObject);
            }
        }
    }
}