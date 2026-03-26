using UnityEngine;
using UnityEngine.UI;

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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseColourPicker()
    {
        colourPickerPanel.SetActive(false);
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
