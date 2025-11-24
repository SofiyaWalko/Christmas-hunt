using UnityEngine;


public class DialogueEventListener : MonoBehaviour
{
    private void OnEnable()
    {
        DialogueManager.OnDialogueEnd += HandleDialogueEnd;
    }


    private void OnDisable()
    {
        DialogueManager.OnDialogueEnd -= HandleDialogueEnd;
    }


    private void HandleDialogueEnd(string dialogueID)
    {
        Debug.Log($"[EventListener] Событие получено! Диалог с ID '{dialogueID}' завершился.");


        // Пример будущей логики для квестов:
        if (dialogueID == "QuestGiver_Intro")
        {
            Debug.LogWarning("[EventListener] Этот диалог должен был запустить квест!");
        }
    }
}