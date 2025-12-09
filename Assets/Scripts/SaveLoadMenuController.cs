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
        _savesContainer = _root.Q<VisualElement>("saves-container");

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

        foreach (var filename in saves)
        {
            var saveData = SaveManager.Instance.GetSaveData(filename);
            if (saveData == null) continue;

            var slot = CreateSaveSlot(filename, saveData);
            _savesContainer.Add(slot);
        }
    }

    private VisualElement CreateSaveSlot(string filename, SaveData data)
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.style.justifyContent = Justify.SpaceBetween;
        container.style.alignItems = Align.Center;
        container.style.paddingBottom = 10;
        container.style.paddingTop = 10;
        container.style.paddingLeft = 10;
        container.style.paddingRight = 10;
        container.style.borderBottomWidth = 1;
        container.style.borderBottomColor = Color.white;
        container.style.height = 60;

        var infoContainer = new VisualElement();
        // "Сохранение <дата>"
        var nameLabel = new Label($"Сохранение {data.timestamp}");
        nameLabel.style.fontSize = 16;
        nameLabel.style.color = Color.white;
        infoContainer.Add(nameLabel);
        container.Add(infoContainer);

        var buttonsContainer = new VisualElement();
        buttonsContainer.style.flexDirection = FlexDirection.Row;

        var loadBtn = new Button(() => OnLoadClicked(filename));
        loadBtn.text = "Загрузить";
        loadBtn.style.marginRight = 5;
        buttonsContainer.Add(loadBtn);

        var delBtn = new Button(() => OnDeleteClicked(filename));
        delBtn.text = "Удалить";
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
