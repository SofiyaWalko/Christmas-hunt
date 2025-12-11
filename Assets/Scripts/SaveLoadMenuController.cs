using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SaveLoadMenuController : MonoBehaviour
{
    [Tooltip("If true, enables Save and Delete All buttons. If false, only shows the list.")]
    public bool isSaveMenu = false;

    private VisualElement _root;
    private VisualElement _savesContainer;

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("SaveLoadMenuController: No UIDocument found!");
            return;
        }

        _root = uiDoc.rootVisualElement;
        // mark this document root as overlay so USS will hide underlying UI/game
        _root.AddToClassList("load-menu");
        _savesContainer = _root.Q<VisualElement>("saves-container");
        // If UXML provides an inner wrapper, prefer that for adding slots
        if (_savesContainer != null)
        {
            var inner = _savesContainer.Q<VisualElement>(className: "saves-inner");
            if (inner != null) _savesContainer = inner;
        }

        if (isSaveMenu)
        {
            var saveBtn = _root.Q<Button>("save-game");
            var deleteBtn = _root.Q<Button>("delete-game"); // Assuming this is the delete all button

            if (saveBtn != null) saveBtn.clicked += OnSaveGameClicked;
            if (deleteBtn != null) deleteBtn.clicked += OnDeleteAllClicked;
        }

        RefreshSaveList();
    }

    private void OnDisable()
    {
        if (isSaveMenu && _root != null)
        {
            var saveBtn = _root.Q<Button>("save-game");
            var deleteBtn = _root.Q<Button>("delete-game");

            if (saveBtn != null) saveBtn.clicked -= OnSaveGameClicked;
            if (deleteBtn != null) deleteBtn.clicked -= OnDeleteAllClicked;
        }
    }

    public void RefreshSaveList()
    {
        if (_savesContainer == null) return;

        _savesContainer.Clear();
        List<string> saves = SaveManager.Instance.GetAllSaveFiles();
        
        // Sort by name (which includes timestamp) descending to show newest first
        saves.Sort(); 
        saves.Reverse(); 

        // prefer inner container if present
        var inner = _savesContainer.Q<VisualElement>(className: "saves-inner");
        VisualElement target = inner ?? _savesContainer;

        foreach (var filename in saves)
        {
            var saveData = SaveManager.Instance.GetSaveData(filename);
            if (saveData == null) continue;

            var slot = CreateSaveSlot(filename, saveData);
            target.Add(slot);
        }
    }

    private VisualElement CreateSaveSlot(string filename, SaveData data)
    {
        var container = new VisualElement();
        container.AddToClassList("save-slot");

        var infoContainer = new VisualElement();
        infoContainer.AddToClassList("save-info");
        // "Сохранение <дата>"
        var nameLabel = new Label($"Сохранение {data.timestamp}");
        nameLabel.AddToClassList("save-name");
        infoContainer.Add(nameLabel);
        container.Add(infoContainer);

        var buttonsContainer = new VisualElement();
        buttonsContainer.AddToClassList("save-buttons");

        var loadBtn = new Button(() => OnLoadClicked(filename));
        loadBtn.text = "Загрузить";
        loadBtn.AddToClassList("save-button");
        buttonsContainer.Add(loadBtn);

        var delBtn = new Button(() => OnDeleteClicked(filename));
        delBtn.text = "Удалить";
        delBtn.AddToClassList("save-button");
        delBtn.AddToClassList("save-delete-button");
        buttonsContainer.Add(delBtn);

        container.Add(buttonsContainer);
        return container;
    }

    private void OnSaveGameClicked()
    {
        SaveManager.Instance.SaveGame();
        RefreshSaveList();
    }

    private void OnDeleteAllClicked()
    {
        SaveManager.Instance.DeleteAllSaves();
        RefreshSaveList();
    }

    private void OnLoadClicked(string filename)
    {
        SaveManager.Instance.LoadGame(filename);
    }

    private void OnDeleteClicked(string filename)
    {
        SaveManager.Instance.DeleteSave(filename);
        RefreshSaveList();
    }
}
