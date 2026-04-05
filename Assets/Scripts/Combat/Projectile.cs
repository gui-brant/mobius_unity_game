using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour, IMovementController
{
    [Header("Projectile Stats")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int baseDamage = 10;

    private Vector2 movementDirection = Vector2.right;
    private GameObject sourceOwner;
    private CombatTeam sourceTeam;
    private int? damageOverride;
    private bool isInitialized;

    private void Awake()
    {
        EnsureProjectilePhysics();
    }

    public void Initialize(
        Vector2 direction,
        GameObject owner,
        CombatTeam team,
        int? runtimeDamageOverride = null)
    {
        sourceOwner = owner;
        sourceTeam = team;
        damageOverride = runtimeDamageOverride;
        SetMovement(direction);
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        transform.position += (Vector3)(movementDirection * speed * Time.deltaTime);
    }

    public void SetMovement(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            movementDirection = Vector2.right;
            return;
        }

        movementDirection = direction.normalized;
    }

    protected Vector2 GetTravelDirection()
    {
        return movementDirection;
    }

    protected void SetRuntimeSpeed(float runtimeSpeed)
    {
        speed = Mathf.Max(0f, runtimeSpeed);
    }

    protected void SetRuntimeDamageOverride(int? runtimeDamage)
    {
        damageOverride = runtimeDamage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized || other == null)
        {
            return;
        }

        GameObject targetObject = ResolveTargetObject(other);
        if (targetObject == null)
        {
            Destroy(gameObject);
            return;
        }

        if (IsOwnerOrSelf(targetObject))
        {
            return;
        }

        if (IsSameTeam(targetObject))
        {
            Destroy(gameObject);
            return;
        }

        HandleTargetHit(targetObject);

        Destroy(gameObject);
    }

    protected virtual void HandleTargetHit(GameObject targetObject)
    {
        IDamageable damageable = GetDamageableFromObject(targetObject);
        if (damageable == null)
        {
            return;
        }

        int finalDamage = damageOverride ?? baseDamage;
        if (finalDamage > 0)
        {
            damageable.TakeDamage(finalDamage);
        }
    }

    private GameObject ResolveTargetObject(Collider2D collider2D)
    {
        if (collider2D.attachedRigidbody != null)
        {
            return collider2D.attachedRigidbody.gameObject;
        }

        return collider2D.gameObject;
    }

    private bool IsOwnerOrSelf(GameObject targetObject)
    {
        if (targetObject == gameObject || targetObject.transform.root == transform.root)
        {
            return true;
        }

        if (sourceOwner == null)
        {
            return false;
        }

        if (targetObject == sourceOwner)
        {
            return true;
        }

        return targetObject.transform.root == sourceOwner.transform.root;
    }

    private bool IsSameTeam(GameObject targetObject)
    {
        ITeamMember targetTeamMember = GetTeamMemberFromObject(targetObject);
        if (targetTeamMember == null)
        {
            return false;
        }

        return targetTeamMember.Team == sourceTeam;
    }

    private IDamageable GetDamageableFromObject(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return null;
        }

        IDamageable damageable = GetInterfaceFromBehaviours<IDamageable>(targetObject.GetComponents<MonoBehaviour>());
        if (damageable != null)
        {
            return damageable;
        }

        Transform current = targetObject.transform.parent;
        while (current != null)
        {
            damageable = GetInterfaceFromBehaviours<IDamageable>(current.GetComponents<MonoBehaviour>());
            if (damageable != null)
            {
                return damageable;
            }

            current = current.parent;
        }

        return null;
    }

    private ITeamMember GetTeamMemberFromObject(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return null;
        }

        ITeamMember teamMember = GetInterfaceFromBehaviours<ITeamMember>(targetObject.GetComponents<MonoBehaviour>());
        if (teamMember != null)
        {
            return teamMember;
        }

        Transform current = targetObject.transform.parent;
        while (current != null)
        {
            teamMember = GetInterfaceFromBehaviours<ITeamMember>(current.GetComponents<MonoBehaviour>());
            if (teamMember != null)
            {
                return teamMember;
            }

            current = current.parent;
        }

        return null;
    }

    protected static T GetInterfaceFromBehaviours<T>(MonoBehaviour[] behaviours) where T : class
    {
        if (behaviours == null)
        {
            return null;
        }

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is T typed)
            {
                return typed;
            }
        }

        return null;
    }

    private void EnsureProjectilePhysics()
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            collider2D.isTrigger = true;
        }

        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            rb2D = gameObject.AddComponent<Rigidbody2D>();
        }

        rb2D.bodyType = RigidbodyType2D.Kinematic;
        rb2D.gravityScale = 0f;
        rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb2D.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
}
