using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string dialogueID;
    public string characterName;

    [TextArea(3, 10)]
    public string[] sentences;

    [TextArea(3, 10)]
    public string[] altSentences;

    public int requiredStatCount = 0;
}
