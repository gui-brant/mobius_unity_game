using UnityEngine;
using UnityEngine.UI;

// Button for Colour Picker UI
public class ColourSwatchButton : MonoBehaviour
{
    public Color colour;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
            ColourPickerUI.Instance.SelectColour(colour));

    }
}
