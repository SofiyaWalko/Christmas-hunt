using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class SaveMenuUIHelper
{
    public static void PopulateSaveList(VisualElement container, bool isSaveMenu, Action onBack, Action onLoad)
    {
        container.Clear();

        // Add Back Button at the top or bottom? 
        // The UXML doesn't have a back button slot, so we'll add it to the container or assume the parent handles it.
        // But for the list itself:
        
        List<string> saves = SaveManager.Instance.GetAllSaveFiles();
        saves.Sort();
        saves.Reverse();

        // prefer to add slots into an inner container (saves-inner) if the UXML provides it
        var inner = container.Q<VisualElement>(className: "saves-inner");
        VisualElement target = inner ?? container;

        foreach (var filename in saves)
        {
            var saveData = SaveManager.Instance.GetSaveData(filename);
            if (saveData == null) continue;

            var slot = CreateSaveSlot(filename, saveData, () => PopulateSaveList(container, isSaveMenu, onBack, onLoad), onLoad, isSaveMenu);
            container.Add(slot);
        }
    }

    private static VisualElement CreateSaveSlot(string filename, SaveData data, Action onRefresh, Action onLoad, bool isSaveMenu)
    {
        var container = new VisualElement();
        container.AddToClassList("save-slot");

        var infoContainer = new VisualElement();
        infoContainer.AddToClassList("save-info");
        var nameLabel = new Label($"Сохранение {data.timestamp}");
        nameLabel.AddToClassList("save-name");
        infoContainer.Add(nameLabel);
        container.Add(infoContainer);

        var buttonsContainer = new VisualElement();
        buttonsContainer.AddToClassList("save-buttons");

        if (!isSaveMenu)
        {
            var loadBtn = new Button(() => {
                SaveManager.Instance.LoadGame(filename, onLoad);
            });
            loadBtn.text = "Загрузить";
            loadBtn.AddToClassList("save-button");
            buttonsContainer.Add(loadBtn);
        }

        var delBtn = new Button(() => {
            SaveManager.Instance.DeleteSave(filename);
            onRefresh?.Invoke();
        });
        delBtn.text = "Удалить";
        delBtn.AddToClassList("save-button");
        delBtn.AddToClassList("save-delete-button");
        buttonsContainer.Add(delBtn);

        container.Add(buttonsContainer);
        return container;
    }
}
