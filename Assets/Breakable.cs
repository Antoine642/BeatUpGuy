using UnityEngine;

public class Breakable : MonoBehaviour
{
    [Header("Break Properties")]
    public int hitsToBreak = 3;
    public float shakeDuration = 0.2f;
    public float shakeIntensity = 0.1f;

    [Header("Effects")]
    public GameObject breakParticles;
    public AudioClip hitSound;
    public AudioClip breakSound;

    private int currentHits = 0;
    private bool isShaking = false;
    private Vector3 originalPosition;
    private float shakeTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    void Start()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Add AudioSource if not present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Handle shaking animation
        if (isShaking)
        {
            shakeTimer -= Time.deltaTime;
            transform.position = originalPosition + Random.insideUnitSphere * shakeIntensity;

            if (shakeTimer <= 0)
            {
                isShaking = false;
                transform.position = originalPosition;
            }
        }
    }

    public void TakeHit()
    {
        currentHits++;

        // Play hit sound
        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Start shaking
        shakeTimer = shakeDuration;
        isShaking = true;

        // Check if block should break
        if (currentHits >= hitsToBreak)
        {
            Break();
        }
    }

    private void Break()
    {
        // Play break sound
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        // Spawn break particles
        if (breakParticles != null)
        {
            Instantiate(breakParticles, transform.position, Quaternion.identity);
        }

        // Destroy the block
        Destroy(gameObject);
    }
}