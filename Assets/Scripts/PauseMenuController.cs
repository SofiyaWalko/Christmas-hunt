using UnityEngine;
using UnityEngine.InputSystem; // Добавляем пространство имен новой системы ввода
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    private VisualElement _root;
    private bool _isPaused;

    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("PauseMenuController: Нет компонента UIDocument!");
            return;
        }

        _root = uiDocument.rootVisualElement;

        // Находим кнопки и подписываемся на события (с проверкой на null)
        var resumeButton = _root.Q<Button>("resume-button");
        if (resumeButton != null)
            resumeButton.clicked += ResumeGame;

        var mainMenuButton = _root.Q<Button>("main-menu-button");
        if (mainMenuButton != null)
            mainMenuButton.clicked += LoadMainMenu;

        var quitButton = _root.Q<Button>("quit-button");
        if (quitButton != null)
            quitButton.clicked += QuitGame;

        // Изначально меню скрыто
        _root.style.display = DisplayStyle.None;
    }

    void Update()
    {
        // ЗАМЕНА: Вместо Input.GetKeyDown(KeyCode.Escape) используем новую систему
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void PauseGame()
    {
        _root.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
        _isPaused = true;
    }

    private void ResumeGame()
    {
        _root.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
        _isPaused = false;
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f; // Важно вернуть время в норму перед сменой сцены
        SceneManager.LoadScene("MainMenu");
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
