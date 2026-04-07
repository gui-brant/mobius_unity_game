using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public float pulseSpeed = 2f;  // How fast it flickers
    public float pulseAmount = 0.05f; // How much it grows/shrinks
    public Vector3 baseScale; // This needs to change when the visibility improves

    void Start()
    {
        // Store the size initially set in the inspector
        baseScale = transform.localScale;
    }

    void Update()
    {
        // Creates a smooth wave between -1 and 1
        float wave = Mathf.Sin(Time.time * pulseSpeed);
        
        // Multiplies the base scale by a small flickering amount
        transform.localScale = baseScale * (1f + wave * pulseAmount);
    }
}