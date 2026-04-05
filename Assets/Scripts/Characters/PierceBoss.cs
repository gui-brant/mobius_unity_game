using System.Collections;
using UnityEngine;

public class PierceBoss : Character, ITargetable, ITeamMember
{
    [Header("Targeting")]
    [SerializeField] private Michael targetMichael;

    [Header("Passive Damage")]
    [SerializeField] private int passiveDamagePerSecond = 10;

    [Header("Reactions")]
    [SerializeField] private float hurtDuration = 1f;       
    [SerializeField] private float awakeDisplayDuration = 1f; 
    [SerializeField] private float dashDistance = 2.5f;        

    [Header("Random Timing")]
    [SerializeField] private float minInterval = 2f;
    [SerializeField] private float maxInterval = 5f;

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
    }

    private void Start()
    {
        // play the Awake animation on spawn
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
        // the boss doesn't move on its own 
        if (isDead)
        {
            HandleDeathState();
        }
    }

    protected override void FixedUpdate()
    {
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

            if (!isDead)
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
            Vector2 destination = (Vector2)transform.position + randomDir * dashDistance;

            if (rb != null)
                rb.position = destination;
            else
                transform.position = (Vector3)destination;
        }

        if (!isDead)
        {
            PlayAnimation("Awake");
            yield return new WaitForSeconds(awakeDisplayDuration);
        }

        if (!isDead)
            PlayAnimation("Idle");

        isMoving = false;
    }

    public override void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;

        health -= amount;

        if (health <= 0)
        {
            health = 0;
            Die();
            return;
        }
    }

    public override void Die()
    {
        if (isDead) return;

        if (SkillTreeManager.instance != null)
            SkillTreeManager.instance.AddPoints(20);

        isDead = true;
        isMoving = false;

        // stop all coroutines so PassiveDamageLoop doesnt fire after death.
        StopAllCoroutines();

        deathTimer = deathAnimationDuration;
        SetMovement(Vector2.zero);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Collider2D hitbox = GetComponent<Collider2D>();
        if (hitbox != null)
            hitbox.enabled = false;

        PlayAnimation("Die");
    }

    protected override void UpdateAnimator() { }

    private void HandleDeathState()
    {
        deathTimer -= Time.deltaTime;

        if (deathTimer <= 0f)
            Destroy(gameObject);
    }
}