using UnityEngine;
using System.Collections;

public class AttachCamera : MonoBehaviour
{
    public string targetName = "Michael"; // name of object to follow
    public Vector3 offset = new Vector3(0, 5, -10);

    void Start()
    {
        // Start a coroutine that waits until the target exists in the scene
        StartCoroutine(AttachWhenReady());
    }

    IEnumerator AttachWhenReady()
    {
        // Wait until the target is found
        Transform target = null;
        while (target == null)
        {
            target = GameObject.Find(targetName)?.transform;
            yield return null; // wait one frame
        }

        // Wait one more frame to ensure the target has moved into position
        yield return null;

        // Attach the camera
        Transform cam = Camera.main.transform;
        cam.SetParent(target);
        cam.localPosition = offset;
        cam.localRotation = Quaternion.identity;
    }
}