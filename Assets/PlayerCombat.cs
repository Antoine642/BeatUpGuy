using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Punch Settings")]
    public KeyCode punchKey = KeyCode.F;
    public float punchRange = 1.0f;
    public LayerMask punchableLayer;
    public Transform punchPoint;

    [Header("Animation")]
    private Animator animator;
    private bool isPunching = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Check for punch input
        if (Input.GetKeyDown(punchKey) && !isPunching)
        {
            Punch();
        }
    }

    void Punch()
    {
        // Trigger punch animation
        animator.SetTrigger("Punch");
        isPunching = true;

        // Check for hit objects in range
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(punchPoint.position, punchRange, punchableLayer);

        // Apply damage to breakable objects
        foreach (Collider2D obj in hitObjects)
        {
            Breakable breakable = obj.GetComponent<Breakable>();
            if (breakable != null)
            {
                breakable.TakeHit();
            }
        }
    }

    // Called by animation event at the end of punch animation
    public void FinishPunchAnimation()
    {
        isPunching = false;
    }

    // Visualize punch range in editor
    private void OnDrawGizmosSelected()
    {
        if (punchPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(punchPoint.position, punchRange);
    }
}