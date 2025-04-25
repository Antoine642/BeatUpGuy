using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Health Display")]
    public GameObject healthContainer;
    public GameObject healthIconPrefab;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    private Image[] healthIcons;

    [Header("Alternative Text Display")]
    public TMP_Text healthText;
    public bool useTextDisplay = false;

    void Awake()
    {
        // If using text display, make sure text component exists
        if (useTextDisplay && healthText == null)
        {
            Debug.LogWarning("Health Text component is missing but useTextDisplay is enabled.");
        }
    }

    public void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        if (useTextDisplay && healthText != null)
        {
            // Update text display
            healthText.text = "Health: " + currentHealth + " / " + maxHealth;
        }
        else if (healthContainer != null)
        {
            // Make sure we have the correct number of health icons
            InitializeHealthIcons(maxHealth);

            // Update the heart sprites based on current health
            for (int i = 0; i < healthIcons.Length; i++)
            {
                if (i < currentHealth)
                {
                    healthIcons[i].sprite = fullHeartSprite;
                }
                else
                {
                    healthIcons[i].sprite = emptyHeartSprite;
                }
            }
        }
    }

    private void InitializeHealthIcons(int maxHealth)
    {
        // If health icons array is null or wrong size, recreate it
        if (healthIcons == null || healthIcons.Length != maxHealth)
        {
            // Clear existing health icons
            foreach (Transform child in healthContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // Create new health icons array
            healthIcons = new Image[maxHealth];

            // Create health icons
            for (int i = 0; i < maxHealth; i++)
            {
                GameObject newIcon = Instantiate(healthIconPrefab, healthContainer.transform);
                healthIcons[i] = newIcon.GetComponent<Image>();
            }
        }
    }
}