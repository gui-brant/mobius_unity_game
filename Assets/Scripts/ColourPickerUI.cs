using UnityEngine;
using UnityEngine.UI;

// UI for User to select what colour they want their character to be

public class ColourPickerUI : MonoBehaviour
{
    public static ColourPickerUI Instance { get; private set; }

    public GameObject colourPickerPanel;
    private SpriteRenderer targetRenderer;
    private Color defaultColour;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OpenColourPicker(SpriteRenderer renderer)
    {
        
        targetRenderer = renderer;
        defaultColour = renderer.color;
        colourPickerPanel.SetActive(true);
        
        // pause
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
    }

    public void CloseColourPicker()
    {
        colourPickerPanel.SetActive(false);
        
        // unpause
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

    public void RevertColour()
    {
        if (targetRenderer != null)
            targetRenderer.color = defaultColour;
        CloseColourPicker();
    }
}
