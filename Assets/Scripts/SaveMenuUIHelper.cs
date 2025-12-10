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

        foreach (var filename in saves)
        {
            var saveData = SaveManager.Instance.GetSaveData(filename);
            if (saveData == null) continue;

            var slot = CreateSaveSlot(filename, saveData, () => PopulateSaveList(container, isSaveMenu, onBack, onLoad), onLoad);
            container.Add(slot);
        }
    }

    private static VisualElement CreateSaveSlot(string filename, SaveData data, Action onRefresh, Action onLoad)
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
        var nameLabel = new Label($"Сохранение {data.timestamp}");
        nameLabel.style.fontSize = 16;
        nameLabel.style.color = Color.white;
        infoContainer.Add(nameLabel);
        container.Add(infoContainer);

        var buttonsContainer = new VisualElement();
        buttonsContainer.style.flexDirection = FlexDirection.Row;

        var loadBtn = new Button(() => {
            SaveManager.Instance.LoadGame(filename);
            onLoad?.Invoke();
        });
        loadBtn.text = "Загрузить";
        loadBtn.style.marginRight = 5;
        buttonsContainer.Add(loadBtn);

        var delBtn = new Button(() => {
            SaveManager.Instance.DeleteSave(filename);
            onRefresh?.Invoke();
        });
        delBtn.text = "Удалить";
        buttonsContainer.Add(delBtn);

        container.Add(buttonsContainer);
        return container;
    }
}
