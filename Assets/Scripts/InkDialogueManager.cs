using System.Collections;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.UIElements;

public class InkDialogueManager : MonoBehaviour
{
    public static InkDialogueManager Instance { get; private set; }

    [SerializeField] public TextAsset inkJSON;
    private Story inkStory;

    // UI элементы
    private VisualElement dialogueBox;
    private Label characterName;
    private Label sentenceText;
    private VisualElement choicesContainer;
    private Button continueButton;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeUI(VisualElement root)
    {
        if (root == null)
        {
            Debug.LogError("Root VisualElement is null!");
            return;
        }

        dialogueBox = root.Q<VisualElement>("ink-dialogue-box");
        characterName = root.Q<Label>("ink-character-name");
        sentenceText = root.Q<Label>("ink-sentence-text");
        choicesContainer = root.Q<VisualElement>("ink-choices-container");
        continueButton = root.Q<Button>("ink-continue-button");

        if (continueButton != null)
        {
            continueButton.clicked += ContinueDialogue;
        }
        
        HideDialogue();
        
        Debug.Log("InkDialogueManager UI initialized successfully!");
    }

    public void StartDialogue(TextAsset inkJSONAsset = null)
    {
        Debug.Log("=== StartDialogue вызван ===");
        
        TextAsset jsonToUse = inkJSONAsset != null ? inkJSONAsset : inkJSON;
        
        Debug.Log($"Используем Ink файл: {jsonToUse.name}");

        try
        {
            Debug.Log("Создание Story...");
            inkStory = new Story(jsonToUse.text);
            Debug.Log($"Story создана. canContinue: {inkStory.canContinue}");
            
            dialogueBox.style.display = DisplayStyle.Flex;
            Debug.Log("Dialogue box показан");
            
            ContinueDialogue();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка: {e.Message}\n{e.StackTrace}");
        }
    }

    private void ContinueDialogue()
    {
        Debug.Log($"ContinueDialogue. canContinue: {inkStory?.canContinue}");

        if (inkStory.canContinue)
        {
            string text = inkStory.Continue();
            
            // Парсим цвета ДО показа
            text = ParseColors(text.Trim());
            
            if (!string.IsNullOrEmpty(text))
            {
                RefreshUI(text);
            }
            else 
            {
                // Пустая строка - продолжаем
                ContinueDialogue();
            }
        }
        else
        {
            Debug.Log("canContinue = false");
            ShowChoicesOrEnd();
        }
    }

    private void RefreshUI(string text)
    {
        choicesContainer.Clear();
        continueButton.style.display = DisplayStyle.None;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        typingCoroutine = StartCoroutine(TypeSentence(text));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        sentenceText.text = "";
        
        // Простая печать (игнорируем теги для скорости)
        bool insideTag = false;
        
        foreach (char letter in sentence)
        {
            if (letter == '<') insideTag = true;
            
            sentenceText.text += letter;
            
            if (letter == '>') insideTag = false;
            
            if (!insideTag)
            {
                yield return new WaitForSeconds(0.03f);
            }
        }
        
        typingCoroutine = null;
        ShowChoicesOrEnd();
    }

    private void ShowChoicesOrEnd()
    {
        choicesContainer.Clear();

        if (inkStory.currentChoices.Count > 0)
        {
            Debug.Log($"Показываем {inkStory.currentChoices.Count} выборов");
            continueButton.style.display = DisplayStyle.None;
            
            foreach (Choice choice in inkStory.currentChoices)
            {
                CreateChoiceButton(choice);
            }
        }
        else if (inkStory.canContinue)
        {
            Debug.Log("Показываем кнопку Continue");
            continueButton.style.display = DisplayStyle.Flex;
        }
        else
        {
            Debug.Log("Диалог завершён");
            StartCoroutine(CloseDialogueAfterDelay(2f));
        }
    }

    private void CreateChoiceButton(Choice choice)
    {
        Button btn = new Button() { text = choice.text };
        btn.clicked += () =>
        {
            Debug.Log($"Выбран вариант: {choice.text}");
            inkStory.ChooseChoiceIndex(choice.index);
            ContinueDialogue();
        };
        btn.AddToClassList("ink-choice-button");
        choicesContainer.Add(btn);
    }

    private string ParseColors(string text)
    {
        return text.Replace("#c:yellow", "<color=#FFD700>")
                   .Replace("#c:green", "<color=#90EE90>")
                   .Replace("#c:red", "<color=#FFB6C1>");
    }

    private IEnumerator CloseDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideDialogue();
    }

    private void HideDialogue()
    {
        if (dialogueBox != null)
        {
            dialogueBox.style.display = DisplayStyle.None;
        }
        
        if (choicesContainer != null)
        {
            choicesContainer.Clear();
        }
        
        inkStory = null;
    }

    public object GetVariable(string variableName)
    {
        if (inkStory != null)
        {
            try
            {
                return inkStory.variablesState[variableName];
            }
            catch (System.Exception)
            {
                Debug.LogWarning($"Переменная '{variableName}' не найдена.");
                return null;
            }
        }
        return null;
    }

    public void SetVariable(string variableName, object value)
    {
        if (inkStory != null)
        {
            try
                {
                    inkStory.variablesState[variableName] = value;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка установки '{variableName}': {e.Message}");
            }
        }
    }
}