using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkullNPC : Character
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    
    protected Rigidbody2D rb;
    protected Vector2 movement;
    
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    
    void Start()
    {
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
        
        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcSprite;
        
        dialoguePanel.SetActive(true);
        
        // pause (from saahil's work)
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char x in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += x;
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }
        
        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        
        //unpause
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }
}
