using UnityEngine;

/*
The class definition currently implemenets a basic enemy.
The enemy will chase Michael on spawn, take damage, attack Michael if close enough, and die when health reaches 0.
Animation handling is also implemented here, although it lacks actual animations. 
*/
public class Enemy : Character, IAttacker, ITargetable
{
    //[SerializeField] allows for you to keep variables private while still being visible on the inspector.
    [Header("Targeting")]
    [SerializeField] private Michael targetMichael;
    [SerializeField] private bool aggroOnSpawn = true;

    [Header("Combat")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 1.25f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackDuration = 0.35f;

    [Header("Reactions")]
    [SerializeField] private float hurtDuration = 0.25f;
    [SerializeField] private float deathAnimationDuration = 0.8f;

    private bool isAggroed;
    private bool isAttacking;
    private bool isHurt;

    private float attackCooldownTimer;
    private float attackTimer;
    private float hurtTimer;
    private float deathTimer;

    public int AttackDamage => attackDamage;
    public Transform TargetTransform => transform;
    public bool CanBeTargeted => !IsDead;

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

        // Check if Michael is within attack range. If so, then attack or move towards him.
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

    /*
    If the enemy is dead, hurt, or attacking, it should not move. 
    This ensures that the enemy is not sliding around during animation. 
    */
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
        //base. allows you to run the original code from the parent class (Character) on top of the new code written here.
        //The basic behaviour allows the Character to move on base update. This means that if the enemy is not in a state, then it may move like any Character.
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
        //The above line sets the movement intent to zero on the frame (within Update()) so that when it is called in the next in-game time tick (on FixedUpdate()), the move() function will be set to zero.
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

        // Enemy death should not halt the whole game.
        // TODO: Add enemy-specific death VFX/SFX hooks here.
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
            PlayAnimation("Death" + direction);
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

/*
Below, you will find the helper funcitons that allow for start attacking, handling mid-attack behaviour
, handling mid-hurt behaviour, and handling mid-death behaviour.
*/
    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;
        attackCooldownTimer = attackCooldown;
        SetMovement(Vector2.zero);

        if (targetMichael != null)
        {
            Attack(targetMichael);
        }
    }

    public void Attack(IDamageable target)
    {
        if (isDead || target == null) return;
        target.TakeDamage(AttackDamage);
    }

    public void ModifyAttackDamage(int amount)
    {
        attackDamage = Mathf.Max(0, attackDamage + amount);
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
