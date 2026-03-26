using UnityEngine;

[CreateAssetMenu(fileName = "NPCDialogue", menuName = "Scriptable Objects/NPCDialogue")]

// General Use Class for NPC Dialogue
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcSprite;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;
    public float typingSpeed = 0.05f;
    public float autoProgressDelay = 1.5f;

    public DialogueChoice[] choices;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes;
}