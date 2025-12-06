using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private Button _startButton;
    private Button _settingsButton;
    private Button _quitButton;

    void OnEnable() // Вызывается, когда объект становится активным
    {
        // Получаем корневой элемент UIDocument
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Находим кнопки по их именам, заданным в UI Builder
        _startButton = root.Q<Button>("start-button");
        _settingsButton = root.Q<Button>("settings-button");
        _quitButton = root.Q<Button>("quit-button");

        // Подписываемся на событие нажатия
        _startButton.clicked += OnStartGame;
        _settingsButton.clicked += OnOpenSettings;
        _quitButton.clicked += OnQuitGame;
    }

    void OnDisable()
    {
        _startButton.clicked -= OnStartGame;
        _settingsButton.clicked -= OnOpenSettings;
        _quitButton.clicked -= OnQuitGame;
    }

    // Укажите имя или индекс сцены с загрузочным экраном
    private void OnStartGame() => SceneManager.LoadScene("LoadingScreen");

    private void OnOpenSettings() => SceneManager.LoadScene("SettingsMenu");

    private void OnQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Для выхода в редакторе
#endif
    }
}
