using UnityEngine;
using UnityEngine.UI;

public class ColourResetButton : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
            ColourPickerUI.Instance.ResetColour());
    }
}
