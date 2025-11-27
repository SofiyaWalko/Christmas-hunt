using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    public Dialogue dialogue;

    [SerializeField]
    private TextAsset inkJSON; //это поле тоже нужно как раз таки для JSON файла в котором лежит сам диалог

    [Tooltip(
        "Если включено, при запуске Ink-диалога будет передан requiredStatCount из Dialogue для проверки переменных внутри ink"
    )]
    public bool useRequiredStatCountForInk = true;

    public void Interact()
    {
        //DialogueManager.Instance.StartDialogue(dialogue); // здесь тоже меняешь местами, когда нужно показать с INK диалогом комментируешь это и расскаментируешь то что ниже, прям оба метода

        // ← НОВОЕ: Ink диалог
        InkDialogueManager.Instance.GetComponent<InkDialogueManager>().inkJSON = inkJSON;
        int statCountRequirement =
            (useRequiredStatCountForInk && dialogue != null) ? dialogue.requiredStatCount : 0;
        InkDialogueManager.Instance.StartDialogue(inkJSON, statCountRequirement);
    }

    public string GetInteractText()
    {
        return "Поговорить с " + dialogue.characterName;
    }
}
