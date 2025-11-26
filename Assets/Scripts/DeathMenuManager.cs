using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class DeathMenuManager : MonoBehaviour
{
    private VisualElement deathMenuContainer;
    private Button restartButton;

    private void OnEnable()
    {
        CharacterStats.OnDeath += ShowDeathMenu;
    }

    private void OnDisable()
    {
        CharacterStats.OnDeath -= ShowDeathMenu;
    }

    private void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("DeathMenuManager: UIDocument component not found!");
            return;
        }

        var root = uiDocument.rootVisualElement;
        deathMenuContainer = root.Q<VisualElement>("death-menu");
        restartButton = root.Q<Button>("restart-button");

        if (deathMenuContainer != null)
        {
            deathMenuContainer.style.display = DisplayStyle.None; // Hide by default
        }
        else
        {
            Debug.LogWarning("DeathMenuManager: 'death-menu' VisualElement not found in UXML.");
        }

        if (restartButton != null)
        {
            restartButton.clicked += OnRestartClicked;
        }
        else
        {
            Debug.LogWarning("DeathMenuManager: 'restart-button' Button not found in UXML.");
        }
    }

    private void ShowDeathMenu()
    {
        if (deathMenuContainer != null)
        {
            deathMenuContainer.style.display = DisplayStyle.Flex;
            
            // Optional: Pause the game or disable controls here if needed
            // Time.timeScale = 0f; 
        }
    }

    private void OnRestartClicked()
    {
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        
        // Reset time scale if it was paused
        // Time.timeScale = 1f;
    }
}
