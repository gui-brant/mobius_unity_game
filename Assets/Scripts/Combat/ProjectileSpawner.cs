using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public Projectile SpawnProjectile(
        GameObject projectilePrefab,
        Transform spawnPoint,
        GameObject sourceOwner,
        CombatTeam sourceTeam,
        float? damageOverride = null,
        Vector2? directionOverride = null)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("SpawnProjectile failed: projectilePrefab is null.");
            return null;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("SpawnProjectile failed: spawnPoint is null.");
            return null;
        }

        if (sourceOwner == null)
        {
            Debug.LogWarning("SpawnProjectile failed: sourceOwner is null.");
            return null;
        }

        GameObject instance = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        if (!instance.TryGetComponent<Projectile>(out Projectile projectile))
        {
            Debug.LogWarning("SpawnProjectile failed: spawned prefab is missing a Projectile component.");
            Destroy(instance);
            return null;
        }

        Vector2 direction = ResolveDirection(spawnPoint, directionOverride);
        int? convertedDamage = damageOverride.HasValue ? Mathf.RoundToInt(damageOverride.Value) : null;
        projectile.Initialize(direction, sourceOwner, sourceTeam, convertedDamage);
        return projectile;
    }

    private Vector2 ResolveDirection(Transform spawnPoint, Vector2? directionOverride)
    {
        if (directionOverride.HasValue && directionOverride.Value.sqrMagnitude > Mathf.Epsilon)
        {
            return directionOverride.Value.normalized;
        }

        Vector2 derived = spawnPoint.right; // The default pawn point is dirctly right of the spawn object.
        if (derived.sqrMagnitude <= Mathf.Epsilon)
        {
            derived = Vector2.right;
        }

        return derived.normalized;
    }
}
