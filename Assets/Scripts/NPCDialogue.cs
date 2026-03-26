using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
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
    
    public int colourPickerDialogueIndex = 2;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes;
}