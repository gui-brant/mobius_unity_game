using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkullNPC : Character
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;
    
    protected Rigidbody2D rb;
    protected Vector2 movement;
    
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    
    public SpriteRenderer playerRenderer;
    public int colourPickerChoiceIndex = 2;
    
    void Start()
    {
        dialogueUI = DialogueController.Instance;
        rb = GetComponent<Rigidbody2D>();
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null)
        {
            return;
        }

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }
    
    

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;
        
        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcSprite);
        
        dialogueUI.ShowDialogueUI(true);
        
        // pause (from saahil's work)
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisplayCurrentLine();
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        // clear choices
        dialogueUI.ClearChoices();
        //check endDialogue
        if (dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }
        // check if choices
        foreach(DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(dialogueChoice);
            }
        }
        
        if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        foreach (char x in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += x);
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }
        
        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            dialogueUI.CreateChoiceButton(choice.choices[i], ()=> ChooseOption(nextIndex));
        }
    }

    void ChooseOption(int nextIndex)
    {
        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        if (nextIndex == colourPickerChoiceIndex)
        {
            EndDialogue();
            ColourPickerUI.Instance.OpenColourPicker(playerRenderer);
            return;
        }
        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        
        //unpause
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }
}
