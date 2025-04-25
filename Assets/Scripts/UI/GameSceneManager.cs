using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    [Header("Loading Screen")]
    public GameObject loadingScreenPrefab;
    public float minimumLoadingTime = 1.0f; // Minimum time to show loading screen

    // Singleton instance
    private static GameSceneManager _instance;
    public static GameSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameSceneManager>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameSceneManager");
                    _instance = obj.AddComponent<GameSceneManager>();
                }
            }
            return _instance;
        }
    }

    private GameObject loadingScreen;

    void Awake()
    {
        // Make this object persist between scenes
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // Load a scene by name with optional loading screen
    public void LoadScene(string sceneName, bool showLoadingScreen = true)
    {
        if (showLoadingScreen)
        {
            StartCoroutine(LoadSceneWithLoadingScreen(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    // Load a scene by build index with optional loading screen
    public void LoadScene(int sceneIndex, bool showLoadingScreen = true)
    {
        if (showLoadingScreen)
        {
            StartCoroutine(LoadSceneWithLoadingScreen(sceneIndex));
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }

    // Reload the current scene
    public void ReloadCurrentScene(bool showLoadingScreen = true)
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex, showLoadingScreen);
    }

    // Load next scene in build settings
    public void LoadNextScene(bool showLoadingScreen = true)
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Check if the next scene exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            LoadScene(nextSceneIndex, showLoadingScreen);
        }
        else
        {
            Debug.LogWarning("Next scene doesn't exist in build settings. Loading first scene.");
            LoadScene(0, showLoadingScreen);
        }
    }

    // Coroutine to load a scene with a loading screen
    private IEnumerator LoadSceneWithLoadingScreen(string sceneName)
    {
        // Create loading screen if it doesn't exist
        CreateLoadingScreen();

        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // Don't activate until we're ready

        float startTime = Time.time;

        // Wait until the scene is loaded
        while (!asyncLoad.isDone)
        {
            // Calculate progress (0 to 0.9 for loading, save 0.1 for scene activation)
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Update loading screen progress
            UpdateLoadingProgress(progress);

            // Check if loading is completed and minimum time has passed
            if (asyncLoad.progress >= 0.9f && (Time.time - startTime) >= minimumLoadingTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Destroy loading screen after scene is loaded
        DestroyLoadingScreen();
    }

    // Overload for loading by index
    private IEnumerator LoadSceneWithLoadingScreen(int sceneIndex)
    {
        // Create loading screen if it doesn't exist
        CreateLoadingScreen();

        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false; // Don't activate until we're ready

        float startTime = Time.time;

        // Wait until the scene is loaded
        while (!asyncLoad.isDone)
        {
            // Calculate progress (0 to 0.9 for loading, save 0.1 for scene activation)
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Update loading screen progress
            UpdateLoadingProgress(progress);

            // Check if loading is completed and minimum time has passed
            if (asyncLoad.progress >= 0.9f && (Time.time - startTime) >= minimumLoadingTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Destroy loading screen after scene is loaded
        DestroyLoadingScreen();
    }

    // Create and show the loading screen
    private void CreateLoadingScreen()
    {
        if (loadingScreenPrefab != null)
        {
            loadingScreen = Instantiate(loadingScreenPrefab);
            DontDestroyOnLoad(loadingScreen);
        }
        else
        {
            // Create a simple loading screen if prefab is not provided
            loadingScreen = new GameObject("LoadingScreen");
            DontDestroyOnLoad(loadingScreen);

            // Create a canvas for the loading screen
            Canvas canvas = loadingScreen.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Make sure it's on top

            // Add a black background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(loadingScreen.transform);
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            UnityEngine.UI.Image bgImage = background.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            // Add loading text
            GameObject textObj = new GameObject("LoadingText");
            textObj.transform.SetParent(loadingScreen.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(300, 100);
            textRect.anchoredPosition = Vector2.zero;

            // Use TextMeshPro if available, otherwise use regular Text
            TMPro.TextMeshProUGUI text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "CHARGEMENT...";
            text.fontSize = 36;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.color = Color.white;
        }
    }

    // Update the loading screen progress
    private void UpdateLoadingProgress(float progress)
    {
        if (loadingScreen != null)
        {
            // Find progress bar or text if it exists
            TMPro.TextMeshProUGUI loadingText = loadingScreen.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (loadingText != null)
            {
                loadingText.text = $"CHARGEMENT... {Mathf.Round(progress * 100)}%";
            }

            // Update loading bar if it exists
            UnityEngine.UI.Slider progressBar = loadingScreen.GetComponentInChildren<UnityEngine.UI.Slider>();
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
        }
    }

    // Destroy the loading screen
    private void DestroyLoadingScreen()
    {
        if (loadingScreen != null)
        {
            Destroy(loadingScreen);
            loadingScreen = null;
        }
    }
}