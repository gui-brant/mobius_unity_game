using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaahilBoss : Character, ITargetable, ITeamMember
{
    [Header("Target")]
    [SerializeField] private Michael targetMichael;

    [Header("Base Stats")]
    [SerializeField] private int bossHealth = 1;
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Combat")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private float stopDistance = 1.2f;

    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float attackDuration = 0.6f;
    [SerializeField] private float windupTime = 0.2f;

    [Header("Scaling Per Attack")]
    [SerializeField] private float cooldownReductionPerAttack = 0.05f;
    [SerializeField] private int damageIncreasePerAttack = 2;

    [Header("Death")]
    [SerializeField] private float deathDuration = 3f;
    [SerializeField] private string nextSceneName = "SampleScene";

    private bool isAttacking = false;
    private bool isDeadLocked = false;

    private float attackCooldownTimer = 0f;
    private float deathTimer = 0f;

    private Vector2 lockedAttackDirection;

    public CombatTeam Team => CombatTeam.Enemy;
    public Transform TargetTransform => transform;
    public bool CanBeTargeted => !IsDead;

    protected override void Awake()
    {
        base.Awake();

        health = bossHealth;
        speed = moveSpeed;

        if (targetMichael == null)
            targetMichael = FindFirstObjectByType<Michael>();
    }

    protected override void Update()
    {
        // 💀 DEATH STATE
        if (isDeadLocked)
        {
            deathTimer -= Time.deltaTime;

            if (deathTimer <= 0f)
            {
                SceneManager.LoadScene(nextSceneName);
            }

            return;
        }

        if (targetMichael == null)
        {
            SetMovement(Vector2.zero);
            UpdateAnimator();
            return;
        }

        attackCooldownTimer -= Time.deltaTime;

        Vector2 toTarget = targetMichael.transform.position - transform.position;
        float distance = toTarget.magnitude;

        // 🚫 KEEP DISTANCE
        if (!isAttacking)
        {
            if (distance > stopDistance)
                SetMovement(toTarget.normalized);
            else
                SetMovement(Vector2.zero);
        }

        // ⚔️ ATTACK
        if (!isAttacking && distance <= attackRange && attackCooldownTimer <= 0f)
        {
            lockedAttackDirection = toTarget.normalized;
            StartCoroutine(Attack());
        }

        UpdateAnimator();
    }

    protected override void FixedUpdate()
    {
        if (isDeadLocked || isAttacking)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        base.FixedUpdate();
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        attackCooldownTimer = attackCooldown;

        SetMovement(Vector2.zero);

        // windup
        yield return new WaitForSeconds(windupTime);

        if (!isDeadLocked && targetMichael != null)
        {
            float dist = Vector2.Distance(transform.position, targetMichael.transform.position);

            if (dist <= attackRange)
                targetMichael.TakeDamage(attackDamage);
        }

        // 🔥 SCALE DIFFICULTY
        attackDamage += damageIncreasePerAttack;
        attackCooldown = Mathf.Max(0.3f, attackCooldown - cooldownReductionPerAttack);

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }

    public override void TakeDamage(int amount)
    {
        if (isDeadLocked || amount <= 0) return;

        Debug.Log("Boss hit for: " + amount);

        health -= amount;

        if (health <= 0)
            Die();
    }

    public override void Die()
    {
        if (isDeadLocked) return;

        Debug.Log("Boss died");

        isDead = true;
        isDeadLocked = true;
        isAttacking = false;

        SetMovement(Vector2.zero);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // 🎬 FORCE death animation
        if (animator != null)
            animator.Play("Death0", 0, 0f);

        // disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // start death timer
        deathTimer = deathDuration;
    }

    protected override void UpdateAnimator()
    {
        if (animator == null) return;

        if (isDeadLocked)
            return;

        int dir = GetDirection();
        int lastDir = GetLastDirection();
        int finalDir = (dir != -1) ? dir : lastDir;

        if (isAttacking)
        {
            int attackDir = GetDirectionFromVector(lockedAttackDirection);
            PlayAnimation("Attack" + attackDir);
            return;
        }

        if (movement == Vector2.zero)
            PlayAnimation("Idle" + lastDir);
        else
            PlayAnimation("Run" + finalDir);
    }

    private int GetDirectionFromVector(Vector2 dir)
    {
        if (dir == Vector2.zero) return 0;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        return Mathf.RoundToInt(angle / 45f) % 8;
    }
}