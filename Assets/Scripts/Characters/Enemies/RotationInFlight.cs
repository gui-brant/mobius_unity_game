using UnityEngine;

public class RotationInFlight : MonoBehaviour
    
{
    [SerializeField] private float rotationSpeed = 360f; // degrees per second

    // Update is called once per frame
    
    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}