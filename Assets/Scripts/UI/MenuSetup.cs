using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class MenuSetup : MonoBehaviour
{
    [Header("References")]
    public MainMenu mainMenu;

    [Header("Prefabs")]
    public GameObject buttonPrefab;

    [Header("Containers")]
    public Transform mainMenuContainer;
    public Transform rulesContainer;

    [Header("Text Styles")]
    public Font menuFont;
    public int titleFontSize = 48;
    public int buttonFontSize = 24;
    public int rulesFontSize = 20;

    private bool hasBeenSetup = false;

    void Awake()
    {
        if (!Application.isPlaying && !hasBeenSetup)
        {
            SetupMenuUI();
        }
    }

    public void SetupMenuUI()
    {
        // Create the main menu panel if it doesn't exist
        if (mainMenu.mainMenuPanel == null)
        {
            GameObject mainMenuObj = new GameObject("MainMenuPanel");
            mainMenuObj.transform.SetParent(transform);
            RectTransform rectTransform = mainMenuObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            mainMenu.mainMenuPanel = mainMenuObj;
            mainMenuContainer = mainMenuObj.transform;
        }

        // Create the rules panel if it doesn't exist
        if (mainMenu.rulesPanel == null)
        {
            GameObject rulesObj = new GameObject("RulesPanel");
            rulesObj.transform.SetParent(transform);
            RectTransform rectTransform = rulesObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            mainMenu.rulesPanel = rulesObj;
            rulesContainer = rulesObj.transform;
        }

        // Setup main menu content
        SetupMainMenuContent();

        // Setup rules content
        SetupRulesContent();

        hasBeenSetup = true;
        Debug.Log("Menu UI setup completed!");
    }

    private void SetupMainMenuContent()
    {
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mainMenuContainer);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(400, 100);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "PLUFORM GAME";
        titleText.fontSize = titleFontSize;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Play Button
        GameObject playButtonObj = CreateButton("PlayButton", "JOUER", new Vector2(0.5f, 0.5f), new Vector2(200, 60));
        playButtonObj.transform.SetParent(mainMenuContainer);
        Button playBtn = playButtonObj.GetComponent<Button>();
        playBtn.onClick.AddListener(mainMenu.PlayGame);
        mainMenu.playButton = playBtn;

        // Rules Button
        GameObject rulesButtonObj = CreateButton("RulesButton", "RÈGLES DU JEU", new Vector2(0.5f, 0.35f), new Vector2(200, 60));
        rulesButtonObj.transform.SetParent(mainMenuContainer);
        Button rulesBtn = rulesButtonObj.GetComponent<Button>();
        rulesBtn.onClick.AddListener(mainMenu.ShowRules);
        mainMenu.rulesButton = rulesBtn;
    }

    private void SetupRulesContent()
    {
        // Title
        GameObject titleObj = new GameObject("RulesTitle");
        titleObj.transform.SetParent(rulesContainer);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(400, 100);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "RÈGLES DU JEU";
        titleText.fontSize = titleFontSize;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Rules text
        GameObject rulesTextObj = new GameObject("RulesText");
        rulesTextObj.transform.SetParent(rulesContainer);

        RectTransform rulesRect = rulesTextObj.AddComponent<RectTransform>();
        rulesRect.anchorMin = new Vector2(0.2f, 0.3f);
        rulesRect.anchorMax = new Vector2(0.8f, 0.8f);
        rulesRect.anchoredPosition = Vector2.zero;
        rulesRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI rulesText = rulesTextObj.AddComponent<TextMeshProUGUI>();
        rulesText.text = "CONTRÔLES :\n\n" +
                         "- ZQSD ou Flèches directionnelles : Se déplacer\n" +
                         "- Espace : Sauter\n" +
                         "- Clic Gauche : Taper/Attaquer\n" +
                         "- V : Cracher\n\n" +
                         "BUT DU JEU :\n\n" +
                         "- Terminer le niveau en évitant les obstacles et en battant les ennemis";
        rulesText.fontSize = rulesFontSize;
        rulesText.alignment = TextAlignmentOptions.Left;
        rulesText.color = Color.white;

        // Back Button
        GameObject backButtonObj = CreateButton("BackButton", "RETOUR", new Vector2(0.5f, 0.15f), new Vector2(200, 60));
        backButtonObj.transform.SetParent(rulesContainer);
        Button backBtn = backButtonObj.GetComponent<Button>();
        backBtn.onClick.AddListener(mainMenu.ShowMainMenu);
        mainMenu.backButton = backBtn;
    }

    private GameObject CreateButton(string name, string text, Vector2 anchorPosition, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);

        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorPosition;
        rectTransform.anchorMax = anchorPosition;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;

        Image image = buttonObj.AddComponent<Image>();
        image.color = mainMenu.buttonBackgroundColor;

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = mainMenu.buttonBackgroundColor;
        colors.highlightedColor = mainMenu.buttonHoverColor;
        colors.pressedColor = mainMenu.buttonHoverColor * 0.8f;
        button.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = buttonFontSize;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = mainMenu.buttonTextColor;

        return buttonObj;
    }
}