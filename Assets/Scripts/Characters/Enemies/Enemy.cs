using System.Collections;
using UnityEngine;

/*
The class definition currently implemenets a basic enemy.
The enemy will chase Michael on spawn, take damage, attack Michael if close enough, and die when health reaches 0.
Animation handling is also implemented here, although it lacks actual animations. 
*/
public class Enemy : Character, ITargetable, ITeamMember
{
    //[SerializeField] allows for you to keep variables private while still being visible on the inspector.
    [Header("Targeting")]
    [SerializeField] protected Michael targetMichael;
    [SerializeField] private bool aggroOnSpawn = true;

    [Header("Combat")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 1.25f;
    [SerializeField] private float attackCooldown = 2.0f; // Adjusted for longer animation
    [SerializeField] private float attackDuration = 1.09f; // Locked duration
    [SerializeField] private float hitDelay = 0.35f;      // Delay before damage

    [Header("Reactions")]
    [SerializeField] private float hurtDuration = 0.25f;
    [SerializeField] private float deathAnimationDuration = 0.8f;

    private bool isAggroed;
    protected bool isAttacking;
    protected bool isHurt;

    private float attackCooldownTimer;
    private float attackTimer;
    private float hurtTimer;
    private float deathTimer;

    public int AttackDamage => attackDamage;
    public CombatTeam Team => CombatTeam.Enemy;
    public Transform TargetTransform => transform;
    public bool CanBeTargeted => !IsDead;

    public virtual void SetTargetMichael(Michael michael)
    {
        targetMichael = michael;
        isAggroed = aggroOnSpawn && targetMichael != null;
    }

    protected override void Awake()
    {
        //awake behaviour definition: chase Michael. 
        base.Awake();

        if (targetMichael == null)
        {
            targetMichael = FindFirstObjectByType<Michael>();
        }

        // If the enemy is set to aggro on spawn, it will start chasing Michael immediately. 
        isAggroed = aggroOnSpawn && targetMichael != null;
    }

    protected override void Update()
    {
        /*Update behaviour definition:
        1. If dead, play death animation and wait for destruction.
        2. If not aggroed or there is no Michael, idle. 
        3. If hurt, play hurt animation and don't move until hurt duration ends. 
        4. If attacking, play attack animation and don't move until attack duration ends.
        5. If the enemy is close enough and attack cooldown is ready, start an attack.
              Otherwise, move towards Michael.
        6. Finally, update the animation with the current state and direction. 
        */
        if (isDead)
        {
            HandleDeathState();
            UpdateAnimator();
            return;
        }

        if (!isAggroed || targetMichael == null)
        {
            SetMovement(Vector2.zero); 
            UpdateAnimator();
            return;
        }

        attackCooldownTimer -= Time.deltaTime; // Countdown attack cooldown regardless of state. 

        if (isHurt)
        {
            HandleHurtState(); 
            UpdateAnimator();
            return;
        }

        if (isAttacking)
        {
            HandleAttackState(); 
            UpdateAnimator();
            return;
        }

        Vector2 toTarget = targetMichael.transform.position - transform.position;
        float distanceToTarget = toTarget.magnitude;

        if (distanceToTarget <= attackRange && attackCooldownTimer <= 0f)
        {
            StartAttack();
        }
        else
        {
            SetMovement(toTarget.normalized);
        }

        UpdateAnimator();
    }

    protected override void FixedUpdate()
    {
        if (isDead || isHurt || isAttacking)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            return;
        }

        base.FixedUpdate();
    }

    public override void TakeDamage(int amount)
    {
        /*
        Take damage handling:
        1. If the enemy is already dead or the damage amount is nothing, ignore. 
        2. Subtract health by damage amount. 
        3. If health drops to 0 or below, die. 
           Otherwise, update isHurt state to true, reset hurt timer, cancel an ongoing attack, and set its movement to zero to prevent sliding. 
        */
        if (isDead || amount <= 0) return;

        health -= amount;

        if (health <= 0)
        {
            health = 0;
            Die();
            return;
        }

        isHurt = true;
        hurtTimer = hurtDuration;
        isAttacking = false;
        attackTimer = 0f;
        SetMovement(Vector2.zero);
    }

    public override void Die()
    {
        /*
        1. If already dead, ignore.
        2. Reset states: the enemy isDead, is not aggroed, not attacking, and not hurt.
        3. Start death timer and set movement's intent to zero (on frame signalling to in-game time).
        4. Stop moving immediately on this in-game time tick. 3. and 4. ensure enemy will not move.
        5. Disable enemy's hitbox.
        */
        if (isDead) return;

        if (SkillTreeManager.instance != null)
        {
            SkillTreeManager.instance.AddPoints(20);
        }

        isDead = true;
        isAggroed = false;
        isAttacking = false;
        isHurt = false;

        deathTimer = deathAnimationDuration;
        SetMovement(Vector2.zero);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Collider2D hitbox = GetComponent<Collider2D>();
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
    }

    protected override void UpdateAnimator()
    {
        /*
        1. Do nothing if Animator animator is not defined in Unity.
        2. If dead, play death animation based on last direction.
        3. If hurt, play hurt animation based on last direction.
        4. If attacking, play attack animation based on last direction.
        5. If none of the above, fallback to Character's base animation handling (movement-based).
        */
        if (animator == null) return;

        int direction = GetLastDirection();

        if (isDead)
        {
            PlayAnimation("Die");
            return;
        }

        if (isHurt)
        {
            PlayAnimation("Hurt" + direction);
            return;
        }

        if (isAttacking)
        {
            PlayAnimation("Attack" + direction);
            return;
        }

        base.UpdateAnimator();
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;
        attackCooldownTimer = attackCooldown;
        SetMovement(Vector2.zero);

        StartCoroutine(DelayedEnemyHit());
    }

    private IEnumerator DelayedEnemyHit()
    {
        yield return new WaitForSeconds(hitDelay);

        if (!isDead && !isHurt && targetMichael != null)
        {
            float dist = Vector2.Distance(transform.position, targetMichael.transform.position);
            if (dist <= attackRange)
            {
                targetMichael.TakeDamage(attackDamage);
            }
        }
    }

    private void HandleAttackState()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            isAttacking = false;
        }
    }

    private void HandleHurtState()
    {
        SetMovement(Vector2.zero);
        hurtTimer -= Time.deltaTime;

        if (hurtTimer <= 0f)
        {
            isHurt = false;
        }
    }

    private void HandleDeathState()
    {
        deathTimer -= Time.deltaTime;

        if (deathTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}