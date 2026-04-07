using System.Collections;
using UnityEngine;

public class PierceBoss : Character, ITargetable, ITeamMember
{
    [Header("Targeting")]
    [SerializeField] private Michael targetMichael;

    [Header("Collision & Targeting")]
    // In the Inspector, set this to "Default" or whatever layers your walls are on
    [SerializeField] private LayerMask collisionLayers; 

    [Header("Passive Damage")]
    [SerializeField] private int passiveDamagePerSecond = 10;

    [Header("Reactions")]
    [SerializeField] private float hurtDuration = 1f;       
    [SerializeField] private float awakeDisplayDuration = 1f; 
    [SerializeField] private float dashDistance = 5f;        
    [SerializeField] private float dashDuration = 0.15f; // Increased to prevent clipping

    [Header("Random Timing")]
    [SerializeField] private float minInterval = 1f;
    [SerializeField] private float maxInterval = 3f;

    [Header("Death")]
    [SerializeField] private float deathAnimationDuration = 1.2f;

    private bool isMoving = false; 
    private float deathTimer;

    public CombatTeam Team => CombatTeam.Enemy;
    public Transform TargetTransform => transform;
    public bool CanBeTargeted => !IsDead;

    protected override void Awake()
    {
        base.Awake();

        if (targetMichael == null)
            targetMichael = FindFirstObjectByType<Michael>();

        // Enable continuous collision to help stop clipping at high speeds
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
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
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector2 start = (Vector2)transform.position;

            // Get the size of our own collider to ensure the CircleCast matches our body
            float radius = 0.3f;
            Collider2D myCol = GetComponent<Collider2D>();
            if (myCol is CircleCollider2D cc) radius = cc.radius * transform.localScale.x;

            // Use the Serialized LayerMask from the Inspector
            // We use a small offset (0.1f) so the boss doesn't get stuck perfectly flush inside a wall
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
        Vector2 start = rb != null ? rb.position : (Vector2)transform.position;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;
            
            // Smoother movement curve
            t = t * t * (3f - 2f * t);

            Vector2 nextPos = Vector2.Lerp(start, destination, t);

            if (rb != null) rb.MovePosition(nextPos);
            else transform.position = nextPos;

            yield return null; 
        }

        if (rb != null) rb.MovePosition(destination);
        else transform.position = destination;
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
    }

    private void HandleDeathState()
    {
        deathTimer -= Time.deltaTime;
        if (deathTimer <= 0f) Destroy(gameObject);
    }

    protected override void FixedUpdate() { }
    protected override void UpdateAnimator() { }
}