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

        // Изначально меню скрыто
        _root.style.display = DisplayStyle.None;
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
                        SaveMenuUIHelper.PopulateSaveList(savesContainer, true, null);
                };
            }
            
            var deleteBtn = subMenu.Q<Button>("delete-game");
            if (deleteBtn != null)
            {
                deleteBtn.clicked += () => {
                    SaveManager.Instance.DeleteAllSaves();
                    if (savesContainer != null)
                        SaveMenuUIHelper.PopulateSaveList(savesContainer, true, null);
                };
            }
        }

        if (savesContainer != null)
        {
            SaveMenuUIHelper.PopulateSaveList(savesContainer, isSaveMenu, null);
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
