using UnityEngine;

public class HybrizoProjectile : Projectile
{
    [Header("Crowd Control")]
    [SerializeField] private float stunDurationSeconds = 0.35f;
    [SerializeField] private float knockBackDistanceUnits = 1.5f;
    [SerializeField] private float knockBackDurationSeconds = 0.18f;
    [SerializeField] private bool allowStunChainByDesign = false;
    [SerializeField] [Min(0.01f)] private float safeMaxStunDurationSeconds = 0.45f;

    public void ConfigureCrowdControl(float stunDuration, float knockBackDistance, float knockBackDuration)
    {
        stunDurationSeconds = Mathf.Max(0f, stunDuration);
        knockBackDistanceUnits = Mathf.Max(0f, knockBackDistance);
        knockBackDurationSeconds = Mathf.Max(0f, knockBackDuration);
        if (!allowStunChainByDesign)
        {
            stunDurationSeconds = Mathf.Min(stunDurationSeconds, Mathf.Max(0.01f, safeMaxStunDurationSeconds));
        }
    }

    public void ConfigureTravelSpeed(float projectileSpeed)
    {
        SetRuntimeSpeed(projectileSpeed);
    }

    public void ConfigureDamage(int projectileDamage)
    {
        SetRuntimeDamageOverride(Mathf.Max(0, projectileDamage));
    }

    protected override void HandleTargetHit(GameObject targetObject)
    {
        base.HandleTargetHit(targetObject);
        ApplyCrowdControl(targetObject);
    }

    private void ApplyCrowdControl(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }

        IStun stun = ResolveInterfaceOnTarget<IStun>(targetObject);
        if (stun != null && stunDurationSeconds > 0f)
        {
            stun.ApplyStun(stunDurationSeconds);
        }

        IKnockBack knockBack = ResolveInterfaceOnTarget<IKnockBack>(targetObject);
        if (knockBack != null && knockBackDistanceUnits > 0f)
        {
            knockBack.ApplyKnockBack(GetTravelDirection(), knockBackDistanceUnits, knockBackDurationSeconds);
        }
    }

    private T ResolveInterfaceOnTarget<T>(GameObject targetObject) where T : class
    {
        T resolved = GetInterfaceFromBehaviours<T>(targetObject.GetComponents<MonoBehaviour>());
        if (resolved != null)
        {
            return resolved;
        }

        Transform current = targetObject.transform.parent;
        while (current != null)
        {
            resolved = GetInterfaceFromBehaviours<T>(current.GetComponents<MonoBehaviour>());
            if (resolved != null)
            {
                return resolved;
            }

            current = current.parent;
        }

        return null;
    }

    private void OnValidate()
    {
        stunDurationSeconds = Mathf.Max(0f, stunDurationSeconds);
        knockBackDistanceUnits = Mathf.Max(0f, knockBackDistanceUnits);
        knockBackDurationSeconds = Mathf.Max(0f, knockBackDurationSeconds);
        safeMaxStunDurationSeconds = Mathf.Max(0.01f, safeMaxStunDurationSeconds);

        if (!allowStunChainByDesign)
        {
            stunDurationSeconds = Mathf.Min(stunDurationSeconds, safeMaxStunDurationSeconds);
        }
    }
}
