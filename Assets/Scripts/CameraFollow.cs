using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // stores reference to transform of target to follow (Michael)
    [SerializeField] private Character targetCharacter;

    private Transform _targetTransform;

    // offset height of character so Michael is always centered
    [SerializeField] private float yOffset = 0.4f;
    
    // ensures quick and smooth movement to follow the main character
    [SerializeField] private float smoothSpeed = 20f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targetCharacter == null)
        {
            targetCharacter = FindObjectOfType<Michael>();
            Debug.Log("Michael found");
        }
        else
        {
            Debug.Log("target given");
        }
        _targetTransform = targetCharacter.transform;
        Debug.Log("target transform found");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // nothing to follow
        if (targetCharacter == null) return;
        
        // setting desired position as target's position
        Vector3 desiredPosition = new  Vector3(
            _targetTransform.position.x,
            _targetTransform.position.y + yOffset,
            transform.position.z);
        
        // Using .lerp for smooth movement - essentially moves halfway each time
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
