using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [Header("Templates")]
    public VisualTreeAsset loadGameMenuTemplate;

    private Button _startButton;
    private Button _loadButton;
    private Button _settingsButton;
    private Button _quitButton;

    private VisualElement _root;
    private VisualElement _mainMenuContainer;

    void OnEnable() // Вызывается, когда объект становится активным
    {
        // Получаем корневой элемент UIDocument
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;

        _root = uiDoc.rootVisualElement;
        _mainMenuContainer = _root.Q<VisualElement>("container");

        // Находим кнопки по их именам, заданным в UI Builder
        _startButton = _root.Q<Button>("start-button");
        _loadButton = _root.Q<Button>("load-button");
        _settingsButton = _root.Q<Button>("settings-button");
        _quitButton = _root.Q<Button>("quit-button");

        // Подписываемся на событие нажатия
        if (_startButton != null) _startButton.clicked += OnStartGame;
        if (_loadButton != null) _loadButton.clicked += OnOpenLoadGame;
        if (_settingsButton != null) _settingsButton.clicked += OnOpenSettings;
        if (_quitButton != null) _quitButton.clicked += OnQuitGame;
    }

    void OnDisable()
    {
        if (_startButton != null) _startButton.clicked -= OnStartGame;
        if (_loadButton != null) _loadButton.clicked -= OnOpenLoadGame;
        if (_settingsButton != null) _settingsButton.clicked -= OnOpenSettings;
        if (_quitButton != null) _quitButton.clicked -= OnQuitGame;
    }

    private void OnOpenLoadGame()
    {
        if (loadGameMenuTemplate == null)
        {
            Debug.LogError("LoadGameMenuTemplate is not assigned in Inspector!");
            return;
        }

        if (_mainMenuContainer != null) _mainMenuContainer.style.display = DisplayStyle.None;

        // Instantiate Load Menu
        VisualElement loadMenu = loadGameMenuTemplate.CloneTree();
        loadMenu.AddToClassList("load-menu");
        loadMenu.style.flexGrow = 1;
        loadMenu.StretchToParentSize();
        _root.Add(loadMenu);

        // Setup List
        var savesContainer = loadMenu.Q<VisualElement>("saves-container");
        if (savesContainer != null)
        {
            SaveMenuUIHelper.PopulateSaveList(savesContainer, false, null, null);
        }

        // Add Back Button
        var backBtn = new Button(() => {
            _root.Remove(loadMenu);
            if (_mainMenuContainer != null) _mainMenuContainer.style.display = DisplayStyle.Flex;
        });
        backBtn.text = "Назад";
        backBtn.style.height = 40;
        backBtn.style.marginTop = 10;
        backBtn.style.fontSize = 20;
        loadMenu.Add(backBtn);
    }

    // Укажите имя или индекс сцены с загрузочным экраном
    private void OnStartGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ClearSessionData();
        }
        SceneManager.LoadScene("LoadingScreen");
    }

    private void OnOpenSettings() => SceneManager.LoadScene("SettingsMenu");

    private void OnQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Для выхода в редакторе
#endif
    }
}
