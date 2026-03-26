using UnityEngine;
using System.Collections;

public class AttachCamera : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 0, -10);

    void Start()
    {
        StartCoroutine(AttachWhenReady());
    }

    IEnumerator AttachWhenReady()
    {
        Character targetCharacter = null;

        // Wait until we find a valid (alive) character
        while (targetCharacter == null || targetCharacter.isDead)
        {
            Character[] allCharacters = FindObjectsByType<Character>(FindObjectsSortMode.None);

            foreach (Character c in allCharacters)
            {
                if (!c.isDead)
                {
                    targetCharacter = c;
                    break;
                }
            }

            yield return null; // wait one frame
        }

        // Ensure position is settled
        yield return null;

        // Attach camera
        Transform cam = Camera.main.transform;
        cam.SetParent(targetCharacter.transform);
        cam.localPosition = offset;
        cam.localRotation = Quaternion.identity;
    }
}