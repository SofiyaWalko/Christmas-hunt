using UnityEngine;


[System.Serializable]
public class Dialogue
{
    public string dialogueID; // Уникальный ID для идентификации диалога
    public string characterName;


    [TextArea(3, 10)] // Удобное поле для ввода текста в инспекторе
    public string[] sentences;
}