using UnityEngine;


public class DialogueTrigger : MonoBehaviour, IInteractable
{
    public Dialogue dialogue;
    [SerializeField] private TextAsset inkJSON; //это поле тоже нужно как раз таки для JSON файла в котором лежит сам диалог

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(dialogue); // здесь тоже меняешь местами, когда нужно показать с INK диалогом комментируешь это и расскаментируешь то что ниже, прям оба метода
        
        // ← НОВОЕ: Ink диалог
        InkDialogueManager.Instance.GetComponent<InkDialogueManager>().inkJSON = inkJSON;
        InkDialogueManager.Instance.StartDialogue();
    }
    
    public string GetInteractText()
    {
        return "Поговорить с " + dialogue.characterName;
    }
}