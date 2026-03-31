using UnityEngine;

public class AttachCamera : MonoBehaviour
{
    [SerializeField] private string targetName = "Michael";
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);

    private Transform target;
    private Character targetCharacter;
    private Transform cameraTransform;

    private void Awake()
    {
        ResolveCamera();
    }

    private void LateUpdate()
    {
        if (!HasValidTarget())
        {
            ResolveTarget();
        }

        if (!HasValidCamera())
        {
            ResolveCamera();
        }

        if (target == null || cameraTransform == null)
        {
            return;
        }

        Vector3 targetPosition = target.position;
        cameraTransform.position = new Vector3(
            targetPosition.x + offset.x,
            targetPosition.y + offset.y,
            targetPosition.z + offset.z
        );
    }

    private bool HasValidTarget()
    {
        return target != null &&
               target.gameObject.activeInHierarchy &&
               targetCharacter != null &&
               !targetCharacter.IsDead;
    }

    private bool HasValidCamera()
    {
        return cameraTransform != null && cameraTransform.gameObject.activeInHierarchy;
    }

    private void ResolveCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.isActiveAndEnabled)
        {
            cameraTransform = mainCamera.transform;
            return;
        }

        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        for (int i = 0; i < allCameras.Length; i++)
        {
            Camera candidate = allCameras[i];
            if (candidate == null || !candidate.isActiveAndEnabled) continue;
            cameraTransform = candidate.transform;
            return;
        }

        cameraTransform = null;
    }

    private void ResolveTarget()
    {
        target = null;
        targetCharacter = null;

        if (!string.IsNullOrWhiteSpace(targetName))
        {
            GameObject namedObject = GameObject.Find(targetName);
            if (namedObject != null &&
                namedObject.activeInHierarchy &&
                namedObject.TryGetComponent<Character>(out Character namedCharacter) &&
                !namedCharacter.IsDead)
            {
                target = namedObject.transform;
                targetCharacter = namedCharacter;
                return;
            }
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            GameObject candidate = players[i];
            if (candidate == null || !candidate.activeInHierarchy) continue;
            if (!candidate.TryGetComponent<Character>(out Character candidateCharacter)) continue;
            if (candidateCharacter.IsDead) continue;

            target = candidate.transform;
            targetCharacter = candidateCharacter;
            return;
        }
    }
}
