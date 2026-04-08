using System.Collections;
using UnityEngine;

public class PierceBoss : Character, ITargetable, ITeamMember
{
    [Header("Targeting")]
    [SerializeField] private Michael targetMichael;

    [Header("Collision & Targeting")]
    [SerializeField] private LayerMask collisionLayers; 
    [SerializeField] private BoxCollider2D topWall;
    [SerializeField] private BoxCollider2D bottomWall;
    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;

    [Header("Passive Damage")]
    [SerializeField] private int passiveDamagePerSecond = 10;

    [Header("Reactions")]
    [SerializeField] private float hurtDuration = 1f;       
    [SerializeField] private float awakeDisplayDuration = 1f; 
    [SerializeField] private float minDashDistance = 2f;
    [SerializeField] private float maxDashDistance = 10f;
    [SerializeField] private float dashDuration = 0.15f; // Increased to prevent clipping

    [Header("Random Timing")]
    [SerializeField] private float minInterval = 1f;
    [SerializeField] private float maxInterval = 3f;

    [Header("Death")]
    [SerializeField] private float deathAnimationDuration = 1.2f;

    private bool isMoving = false; 
    private float deathTimer;
    private readonly Collider2D[] assignedWalls = new Collider2D[4];
    
    private MoveScene moveScene;
    private bool transitionTriggered = false;

    public CombatTeam Team => CombatTeam.Enemy;
    public Transform TargetTransform => transform;
    public bool CanBeTargeted => !IsDead;
    private IEnumerator HandleWinTransition()
    {
        Debug.Log("🕒 Player survived! Switching scene...");

        yield return new WaitForSeconds(1f);

        moveScene = FindFirstObjectByType<MoveScene>();

        if (moveScene == null)
        {
            Debug.LogError("MoveScene NOT FOUND");
            yield break;
        }

        yield return moveScene.StartCoroutine(
            moveScene.TransitionProcess("(PGR) Procedurally generated rooms")
        );
    }
    
    protected override void Awake()
    {
        base.Awake();

        if (targetMichael == null)
            targetMichael = FindFirstObjectByType<Michael>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        assignedWalls[0] = topWall;
        assignedWalls[1] = bottomWall;
        assignedWalls[2] = leftWall;
        assignedWalls[3] = rightWall;
    }

    private void Start()
    {
        StartCoroutine(StartupSequence());
    }

    private IEnumerator StartupSequence()
    {
        PlayAnimation("Awake");
        yield return new WaitForSeconds(awakeDisplayDuration);
        PlayAnimation("Idle");
        
        StartCoroutine(PassiveDamageLoop());
        StartCoroutine(QuirkLoop());
    }

    protected override void Update()
    {
        if (isDead)
        {
            HandleDeathState();
        }
    }

    private IEnumerator PassiveDamageLoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(1f);
            if (!isDead && targetMichael != null)
                targetMichael.TakeDamage(passiveDamagePerSecond);
        }
    }

    private IEnumerator QuirkLoop()
    {
        while (!isDead)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            if (!isDead && !isMoving)
                yield return StartCoroutine(MoveSequence());
        }
    }

    private IEnumerator MoveSequence()
    {
        isMoving = true;
        PlayAnimation("Hurt");
        yield return new WaitForSeconds(hurtDuration);
        
        if (!isDead)
        {
            Vector2 randomDir = Random.insideUnitCircle;
            if (randomDir.sqrMagnitude <= Mathf.Epsilon)
                randomDir = Vector2.right;
            else
                randomDir.Normalize();
            Vector2 start = (Vector2)transform.position;
            float dashDistance = Random.Range(
                Mathf.Min(minDashDistance, maxDashDistance),
                Mathf.Max(minDashDistance, maxDashDistance));

            float radius = 0.3f;
            Collider2D myCol = GetComponent<Collider2D>();
            if (myCol is CircleCollider2D cc) radius = cc.radius * transform.localScale.x;

            RaycastHit2D hit = Physics2D.CircleCast(start, radius, randomDir, dashDistance, collisionLayers);

            float safeDistance = (hit.collider != null) ? Mathf.Max(0f, hit.distance - 0.1f) : dashDistance;

            if (safeDistance > 0.1f) 
            {
                Vector2 destination = start + randomDir * safeDistance;
                yield return StartCoroutine(DashTo(destination));
            }
        }

        if (!isDead)
        {
            PlayAnimation("Awake");
            yield return new WaitForSeconds(awakeDisplayDuration);
            PlayAnimation("Idle");
        }

        isMoving = false;
    }

    private IEnumerator DashTo(Vector2 destination)
    {
        float elapsed = 0f;
        Vector2 start = (Vector2)transform.position;
        Collider2D myCol = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;
            
            // Smoother movement curve
            t = t * t * (3f - 2f * t);

            Vector2 currentPos = (Vector2)transform.position;
            Vector2 nextPos = Vector2.Lerp(start, destination, t);
            Vector2 delta = nextPos - currentPos;

            if (delta.sqrMagnitude > 0f)
            {
                nextPos = GetSafeDashPosition(currentPos, nextPos, myCol);
            }

            transform.position = nextPos;
            if (rb != null)
            {
                rb.position = nextPos;
            }

            yield return null; 
        }

        Vector2 finalPosition = GetSafeDashPosition((Vector2)transform.position, destination, myCol);
        transform.position = finalPosition;
        if (rb != null)
        {
            rb.position = finalPosition;
        }
    }

    private Vector2 GetSafeDashPosition(Vector2 from, Vector2 to, Collider2D myCollider)
    {
        if (myCollider == null)
        {
            return to;
        }

        Vector2 delta = to - from;
        float distance = delta.magnitude;
        if (distance <= Mathf.Epsilon)
        {
            return to;
        }

        Vector2 direction = delta / distance;
        Vector2 safePos = to;

        RaycastHit2D layerHit = Physics2D.CircleCast(from, GetColliderRadius(myCollider), direction, distance, collisionLayers);
        if (layerHit.collider != null)
        {
            safePos = from + direction * Mathf.Max(0f, layerHit.distance - 0.05f);
        }

        return ClampPositionInsideAssignedWalls(safePos, myCollider);
    }

    private Vector2 ClampPositionInsideAssignedWalls(Vector2 targetPosition, Collider2D myCollider)
    {
        if (myCollider == null)
        {
            return targetPosition;
        }

        Bounds currentBounds = myCollider.bounds;
        Vector2 extents = currentBounds.extents;
        const float skin = 0.02f;

        float minX = float.NegativeInfinity;
        float maxX = float.PositiveInfinity;
        float minY = float.NegativeInfinity;
        float maxY = float.PositiveInfinity;

        if (leftWall != null)
        {
            minX = leftWall.bounds.max.x + extents.x + skin;
        }

        if (rightWall != null)
        {
            maxX = rightWall.bounds.min.x - extents.x - skin;
        }

        if (bottomWall != null)
        {
            minY = bottomWall.bounds.max.y + extents.y + skin;
        }

        if (topWall != null)
        {
            maxY = topWall.bounds.min.y - extents.y - skin;
        }

        return new Vector2(
            Mathf.Clamp(targetPosition.x, minX, maxX),
            Mathf.Clamp(targetPosition.y, minY, maxY));
    }

    private float GetColliderRadius(Collider2D myCollider)
    {
        if (myCollider is CircleCollider2D circle)
        {
            return circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
        }

        Bounds bounds = myCollider.bounds;
        return Mathf.Max(bounds.extents.x, bounds.extents.y);
    }

    public override void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;
        health -= amount;
        if (health <= 0) Die();
    }

    public override void Die()
    {
        if (isDead) return;
        isDead = true;
        isMoving = false;

        if (SkillTreeManager.instance != null)
            SkillTreeManager.instance.AddPoints(20);

        StopAllCoroutines();
        deathTimer = deathAnimationDuration;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        Collider2D hitbox = GetComponent<Collider2D>();
        if (hitbox != null) hitbox.enabled = false;

        PlayAnimation("Die");

        HandleWinTransition();

    }

    private void HandleDeathState()
    {
        deathTimer -= Time.deltaTime;
        if (deathTimer <= 0f) Destroy(gameObject);
    }

    protected override void FixedUpdate() { }
    protected override void UpdateAnimator() { }
}
