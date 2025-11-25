using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public static event Action<string> OnDialogueStart;
    public static event Action<string> OnDialogueEnd;

    private VisualElement dialogueBox;
    private Label nameText;
    private Label sentenceText;
    private Button continueButton;

    private Queue<string> sentences;
    private string currentDialogueID;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        sentences = new Queue<string>();
    }

    public void Initialize(VisualElement rootElement)
    {
        dialogueBox = rootElement.Q<VisualElement>("dialogue-box");
        nameText = rootElement.Q<Label>("character-name");
        sentenceText = rootElement.Q<Label>("sentence-text");
        continueButton = rootElement.Q<Button>("continue-button");

        continueButton.clicked += DisplayNextSentence;
        dialogueBox.style.display = DisplayStyle.None;
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        sentenceText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            sentenceText.text += letter;
            yield return new WaitForSeconds(0.04f);
        }
    }

    private void EndDialogue()
    {
        dialogueBox.style.display = DisplayStyle.None;
        OnDialogueEnd?.Invoke(currentDialogueID);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        currentDialogueID = dialogue.dialogueID;
        OnDialogueStart?.Invoke(currentDialogueID);

        dialogueBox.style.display = DisplayStyle.Flex;
        nameText.text = dialogue.characterName;

        sentences.Clear();
        // Выбираем набор фраз в зависимости от наличия stat'ов у игрока
        bool useAlt = false;
        if (
            dialogue.altSentences != null
            && dialogue.altSentences.Length > 0
            && dialogue.requiredStatCount > 0
        )
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.HasStat())
            {
                var ss = InventoryManager.Instance.statSlot;
                if (ss != null && ss.quantity >= dialogue.requiredStatCount)
                    useAlt = true;
            }
        }

        var chosen = useAlt ? dialogue.altSentences : dialogue.sentences;
        if (chosen != null)
        {
            foreach (string sentence in chosen)
                sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }
}
