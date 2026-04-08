using UnityEngine;
using UnityEngine.UI;

public class ColourPickerUI : MonoBehaviour
{
    public static ColourPickerUI Instance { get; private set; }

    public GameObject colourPickerPanel;
    private SpriteRenderer targetRenderer;
    private Color originalColour;
    private bool colourStored =  false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OpenColourPicker(SpriteRenderer renderer)
    {
        // pause (from saahil's work)
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        targetRenderer = renderer;
        if (!colourStored)
        {
            originalColour = targetRenderer.color;
            colourStored = true;
        }
        
        colourPickerPanel.SetActive(true);
    }


    public void CloseColourPicker()
    {
        colourPickerPanel.SetActive(false);
        
        //unpause
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SelectColour(Color colour)
    {
        if (targetRenderer != null)
            targetRenderer.color = colour;

        CloseColourPicker();
    }

    public void ResetColour()
    {
        if (targetRenderer != null)
            targetRenderer.color = originalColour;
        
        CloseColourPicker();
    }
}