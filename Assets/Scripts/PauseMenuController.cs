using UnityEngine;
using UnityEngine.InputSystem; // Добавляем пространство имен новой системы ввода
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    [Header("Templates")]
    public VisualTreeAsset saveGameMenuTemplate;
    public VisualTreeAsset loadGameMenuTemplate;

    private VisualElement _root;
    private VisualElement _pauseMenuContainer;
    private bool _isDead;
    private bool _isPaused;

    private void OnEnable()
    {
        CharacterStats.OnDeath += ShowDeathMenu;
    }

    private void OnDisable()
    {
        CharacterStats.OnDeath -= ShowDeathMenu;
    }

    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("PauseMenuController: Нет компонента UIDocument!");
            return;
        }

        _root = uiDocument.rootVisualElement;
        _pauseMenuContainer = _root.Q<VisualElement>("container");

        // Находим кнопки и подписываемся на события (с проверкой на null)
        var resumeButton = _root.Q<Button>("resume-button");
        if (resumeButton != null)
            resumeButton.clicked += ResumeGame;

        var loadButton = _root.Q<Button>("load-button");
        if (loadButton != null)
            loadButton.clicked += OnOpenLoadGame;

        var saveButton = _root.Q<Button>("save-button");
        if (saveButton != null)
            saveButton.clicked += OnOpenSaveGame;

        var mainMenuButton = _root.Q<Button>("main-menu-button");
        if (mainMenuButton != null)
            mainMenuButton.clicked += LoadMainMenu;

        var quitButton = _root.Q<Button>("quit-button");
        if (quitButton != null)
            quitButton.clicked += QuitGame;

        // Попытка найти кнопку рестарта, если она есть в UI (например, добавленная для экрана смерти)
        var restartButton = _root.Q<Button>("restart-button");
        if (restartButton != null)
            restartButton.clicked += RestartGame;

        // Изначально меню скрыто
        _root.style.display = DisplayStyle.None;
    }

    private void ShowDeathMenu()
    {
        _isDead = true;
        _root.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
        _isPaused = true;

        // Скрываем кнопку "Продолжить", так как персонаж мертв
        var resumeButton = _root.Q<Button>("resume-button");
        if (resumeButton != null) resumeButton.style.display = DisplayStyle.None;
        
        // Показываем кнопку рестарта, если она была скрыта
        var restartButton = _root.Q<Button>("restart-button");
        if (restartButton != null) restartButton.style.display = DisplayStyle.Flex;

        // Можно также поменять заголовок меню, если есть такой элемент
        var titleLabel = _root.Q<Label>("title"); // Предполагаемое имя
        if (titleLabel != null) titleLabel.text = "YOU DIED";
    }

    private void OnOpenSaveGame()
    {
        OpenSubMenu(saveGameMenuTemplate, true);
    }

    private void OnOpenLoadGame()
    {
        OpenSubMenu(loadGameMenuTemplate, false);
    }

    private void OpenSubMenu(VisualTreeAsset template, bool isSaveMenu)
    {
        if (template == null)
        {
            Debug.LogError("Template not assigned in Inspector!");
            return;
        }

        if (_pauseMenuContainer != null) _pauseMenuContainer.style.display = DisplayStyle.None;

        VisualElement subMenu = template.CloneTree();
        // mark as overlay so USS can fully cover underlying UI/game
        subMenu.AddToClassList("load-menu");
        subMenu.style.flexGrow = 1;
        subMenu.StretchToParentSize();
        // Ensure a darker background programmatically as fallback
        subMenu.style.backgroundColor = new StyleColor(new Color(0.08f, 0.08f, 0.12f, 1f));
        _root.Add(subMenu);

        var savesContainer = subMenu.Q<VisualElement>("saves-container");
        
        // Setup Save Button if in Save Menu
        if (isSaveMenu)
        {
            var saveBtn = subMenu.Q<Button>("save-game");
            if (saveBtn != null)
            {
                saveBtn.clicked += () => {
                    SaveManager.Instance.SaveGame();
                    if (savesContainer != null)
                        SaveMenuUIHelper.PopulateSaveList(savesContainer, true, null, null);
                };
            }
            
            var deleteBtn = subMenu.Q<Button>("delete-game");
            if (deleteBtn != null)
            {
                deleteBtn.clicked += () => {
                    SaveManager.Instance.DeleteAllSaves();
                    if (savesContainer != null)
                        SaveMenuUIHelper.PopulateSaveList(savesContainer, true, null, null);
                };
            }
        }

        if (savesContainer != null)
        {
            // Pass a lambda that forces the menu to close, ignoring death state
            SaveMenuUIHelper.PopulateSaveList(savesContainer, isSaveMenu, null, () => {
                _isDead = false; // Reset death flag as we are loading a save
                ResumeGame();
            });
        }

        // Back Button
        var backBtn = new Button(() => {
            _root.Remove(subMenu);
            if (_pauseMenuContainer != null) _pauseMenuContainer.style.display = DisplayStyle.Flex;
        });
        backBtn.text = "Назад";
        backBtn.style.height = 40;
        backBtn.style.marginTop = 10;
        backBtn.style.fontSize = 20;
        subMenu.Add(backBtn);
    }

    void Update()
    {
        // Если игрок мертв, не даем закрыть меню через Escape
        if (_isDead) return;

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
        
        // Убеждаемся, что кнопка продолжить видна при обычной паузе
        var resumeButton = _root.Q<Button>("resume-button");
        if (resumeButton != null) resumeButton.style.display = DisplayStyle.Flex;
    }

    private void ResumeGame()
    {
        if (_isDead) return; // На всякий случай

        _root.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
        _isPaused = false;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
