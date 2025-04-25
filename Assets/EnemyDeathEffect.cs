using UnityEngine;

public class EnemyDeathEffect : MonoBehaviour
{
    public float lifetime = 1f;
    public float expandSpeed = 2f;
    public float fadeSpeed = 2f;

    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;

        // Destroy the effect after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Expand effect
        transform.localScale += initialScale * expandSpeed * Time.deltaTime;

        // Fade out
        Color color = spriteRenderer.color;
        color.a -= fadeSpeed * Time.deltaTime;
        spriteRenderer.color = color;
    }
}