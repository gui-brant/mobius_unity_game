using UnityEngine;

public class OrbitalProjectile : MonoBehaviour
{
    [SerializeField] private float orbitSpeed = 90f; // degrees per second
    [SerializeField] private float orbitRadius = 2f;

    private Transform target;
    private float currentAngle;

    public void Initialize(Transform orbitTarget, float startAngle)
    {
        target = orbitTarget;
        currentAngle = startAngle;
    }

    private void Update()
    {
        
        if (target == null) return;
       

        currentAngle += orbitSpeed * Time.deltaTime;
        float rad = currentAngle * Mathf.Deg2Rad;
        transform.position = target.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
    }
}