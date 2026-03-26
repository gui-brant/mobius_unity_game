using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }
    
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public Transform choiceContainer;
    public GameObject choiceButton;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDialogueUI(bool show)
    {
        dialoguePanel.SetActive(show);
    }

    public void SetNPCInfo(string npcName, Sprite npcPortrait)
    {
        dialogueText.text = npcName;
        portraitImage.sprite = npcPortrait;
    }
    
    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }
    
    public void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject button = Instantiate(choiceButton, choiceContainer);
        button.GetComponentInChildren<TMP_Text>().text = choiceText;
        button.GetComponent<Button>().onClick.AddListener(onClick);
    }
}
