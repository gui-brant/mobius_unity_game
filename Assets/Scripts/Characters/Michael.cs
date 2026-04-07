using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Michael : Character, ITargetable, ITeamMember, IAttacker, IStun, IKnockBack
{
    private enum WeaponMode
    {
        Melee = 0,
        Ranged = 1
    }

    [Header("Default Weapon (Melee)")]
    [SerializeField] private int defaultMeleeDamage = 20;
    [SerializeField] private float defaultMeleeRange = 1.25f;
    [SerializeField] private float defaultMeleeHitRadius = 0.45f;

    [Header("Attack Timing")]
    [SerializeField] private float attackWindupDelay = 0.35f;

    [Header("Ranged")]
    [SerializeField] private ProjectileSpawner projectileSpawner;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private LayerMask attackableLayers = ~0;

    [Header("Defense")]
    [SerializeField] private int armor = 0;
    [SerializeField] [Min(0f)] private float stunReapplyLockoutSeconds = 0.2f;

    private bool isAttacking = false;
    private bool isStunned = false;
    private float stunTimer = 0f;
    private float stunReapplyLockoutTimer = 0f;
    private bool isKnockedBack = false;
    private float knockBackTimer = 0f;
    private Vector2 knockBackVelocity = Vector2.zero;
    private readonly HashSet<string> objectiveItems = new HashSet<string>();

    private WeaponMode currentWeaponMode = WeaponMode.Melee;
    private int meleeBaseDamage;
    private float meleeBaseRange;
    private float meleeBaseHitRadius;
    private GameObject rangedProjectilePrefab;
    private int rangedBaseDamage;

    private int bonusAttackDamage;
    private float bonusAttackRange;

    public int AttackDamage => Mathf.Max(0, GetActiveBaseDamage() + bonusAttackDamage);
    public float AttackRange => Mathf.Max(0.1f, meleeBaseRange + bonusAttackRange);
    public int Armor => armor;
    public CombatTeam Team => CombatTeam.Player;
    public Transform TargetTransform => transform;
    public bool CanBeTargeted => !IsDead;

    [Header("Crowd Control Debug")]
    [SerializeField] private bool debugIsStunned;
    [SerializeField] private float debugStunTimer;
    [SerializeField] private bool debugIsKnockedBack;
    [SerializeField] private float debugKnockBackTimer;

    private SkullNPC interactableSkullNPC;

    protected override void Update()
    {
        if (IsDead) return;

        if (Input.GetKeyDown(KeyCode.X)) TakeDamage(9999); // kys button

        UpdateCrowdControlTimers();
        HandleInput();
        HandleAttack();
        base.Update();
        
        HandleInteract();
        
        if (interactableSkullNPC != null && Input.GetKeyDown(KeyCode.E))
        {
            interactableSkullNPC.Interact();
        }
    }

    private void HandleInteract()
    {
        // only check if E is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E is pressed");
            float interactRadius = 0.8f; 
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius);

            
            foreach (var col in colliders)
            {
                // Look for the component (Interface)
                IInteractable interactable = col.gameObject.GetComponent<IInteractable>();

                
                // Only representing Torch interactions for now
                if (interactable != null && interactable is Torch)
                {
                    interactable.Interact(this.gameObject);
                    break; // Stop after interacting with the first valid object
                    // Ensuring only one interaction at a time
                }
            }

        }
    }

    // movement 
    private Vector2 lastEnqueuedInput = Vector2.zero;
    private Queue<(Vector2 input, float enqueueTime)> inputQueue = new Queue<(Vector2, float)>();

    private void HandleInput()
    {
        if (isAttacking || isStunned || isKnockedBack)
        {
            SetMovement(Vector2.zero);
            inputQueue.Clear();
            return;
        }

        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        // only enqueue when input changes
        if (input != lastEnqueuedInput)
        {
            inputQueue.Enqueue((input, Time.time));
            lastEnqueuedInput = input;
        }

        // apply oldest input if its delay has passed, using current inputDelay value
        if (inputQueue.Count > 0 && Time.time >= inputQueue.Peek().enqueueTime + inputDelay)
        {
            SetMovement(inputQueue.Dequeue().input);
        }
    }

    // interacting with skull npc
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<SkullNPC>(out SkullNPC skull))
        {
            interactableSkullNPC = skull;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<SkullNPC>(out SkullNPC skull))
        {
            interactableSkullNPC = null;
        }
    }

    //attack system
    private void HandleAttack()
    {
        if (isStunned || isKnockedBack)
        {
            isAttacking = false;
            return;
        }

        bool holding = Input.GetKey(KeyCode.Space);

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // start attack if not already attacking
        if (holding && !isAttacking)
        {
            StartAttack();
        }

        // check if current animation finished
        if (isAttacking && state.IsName("Attack" + GetLastDirection()) && state.normalizedTime >= 1f)
        {
            isAttacking = false;
        }
    }


    private void StartAttack()
    {
        if (isStunned || isKnockedBack)
        {
            return;
        }

        isAttacking = true;
        int direction = GetDirection();
        if (direction == -1) direction = GetLastDirection();

        string animName = "Attack" + direction;
        PlayAnimation(animName);

        // Change: Start a coroutine instead of calling PerformAttackHit immediately
        StartCoroutine(DelayedAttackHit(direction));
    }

    private IEnumerator DelayedAttackHit(int direction)
    {
        yield return new WaitForSeconds(attackWindupDelay);
        PerformAttackAction(direction);
    }

    public void Attack(IDamageable target)
    {
        if (isDead || target == null) return;
        target.TakeDamage(AttackDamage);
    }

    private void PerformAttackAction(int direction)
    {
        if (currentWeaponMode == WeaponMode.Ranged)
        {
            PerformRangedAttack(direction);
            return;
        }

        PerformMeleeAttack(direction);
    }

    private void PerformMeleeAttack(int direction)
    {
        Vector2 directionVector = DirectionToVector(direction);
        Vector2 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Vector2 hitCenter = origin + (directionVector * AttackRange);

        float effectiveHitRadius = Mathf.Max(0.05f, meleeBaseHitRadius);
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, effectiveHitRadius, attackableLayers);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            GameObject targetObject = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
            if (targetObject == gameObject) continue;

            IDamageable damageable = GetDamageableFromObject(targetObject);
            if (damageable == null || damagedTargets.Contains(damageable)) continue;

            Attack(damageable);
            damagedTargets.Add(damageable);
        }
    }

    private void PerformRangedAttack(int direction)
    {
        if (rangedProjectilePrefab == null)
        {
            Debug.LogWarning("Ranged attack requested, but no projectile prefab is equipped.");
            return;
        }

        ProjectileSpawner spawner = ResolveProjectileSpawner();
        if (spawner == null)
        {
            Debug.LogWarning("Ranged attack requested, but no ProjectileSpawner is available.");
            return;
        }

        Transform spawnPoint = attackOrigin != null ? attackOrigin : transform;
        Vector2 directionVector = DirectionToVector(direction);
        spawner.SpawnProjectile(
            rangedProjectilePrefab,
            spawnPoint,
            gameObject,
            Team,
            AttackDamage,
            directionVector
        );
    }

    private Vector2 DirectionToVector(int direction)
    {
        return direction switch
        {
            0 => Vector2.right,
            1 => new Vector2(1f, 1f).normalized,
            2 => Vector2.up,
            3 => new Vector2(-1f, 1f).normalized,
            4 => Vector2.left,
            5 => new Vector2(-1f, -1f).normalized,
            6 => Vector2.down,
            7 => new Vector2(1f, -1f).normalized,
            _ => Vector2.right
        };
    }

    private IDamageable GetDamageableFromObject(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return null;
        }

        IDamageable damageable = GetDamageableFromBehaviours(targetObject.GetComponents<MonoBehaviour>());
        if (damageable != null)
        {
            return damageable;
        }

        Transform parent = targetObject.transform.parent;
        while (parent != null)
        {
            damageable = GetDamageableFromBehaviours(parent.GetComponents<MonoBehaviour>());
            if (damageable != null)
            {
                return damageable;
            }

            parent = parent.parent;
        }

        return null;
    }

    private IDamageable GetDamageableFromBehaviours(MonoBehaviour[] behaviours)
    {
        if (behaviours == null) return null;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IDamageable damageable)
            {
                return damageable;
            }
        }

        return null;
    }

    public override void TakeDamage(int amount)
    {
        int reducedDamage = Mathf.Max(0, amount - armor);
        if (reducedDamage <= 0) return;

        base.TakeDamage(reducedDamage);
    }

    public void ModifyAttackDamage(int amount)
    {
        bonusAttackDamage = Mathf.Max(0, bonusAttackDamage + amount);
    }

    public void ModifyAttackRange(float amount)
    {
        bonusAttackRange = Mathf.Max(0f, bonusAttackRange + amount);
    }

    public void EquipMeleeWeapon(int baseDamage, float baseRange, float hitRadius)
    {
        currentWeaponMode = WeaponMode.Melee;
        meleeBaseDamage = Mathf.Max(0, baseDamage);
        meleeBaseRange = Mathf.Max(0.1f, baseRange);
        meleeBaseHitRadius = Mathf.Max(0.05f, hitRadius);
    }

    public void EquipRangedWeapon(GameObject projectilePrefab, int projectileDamage)
    {
        currentWeaponMode = WeaponMode.Ranged;
        rangedProjectilePrefab = projectilePrefab;
        rangedBaseDamage = Mathf.Max(0, projectileDamage);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        health += amount;
    }

    public void AddArmor(int amount)
    {
        armor = Mathf.Max(0, armor + amount);
    }

    public void ReceiveObjectiveItem(string objectiveId)
    {
        if (string.IsNullOrWhiteSpace(objectiveId)) return;
        objectiveItems.Add(objectiveId);
    }

    public bool HasObjectiveItem(string objectiveId)
    {
        if (string.IsNullOrWhiteSpace(objectiveId)) return false;
        return objectiveItems.Contains(objectiveId);
    }

    public void ApplyStun(float durationSeconds)
    {
        float finalDuration = Mathf.Max(0f, durationSeconds);
        if (finalDuration <= 0f)
        {
            return;
        }

        if (isStunned && stunReapplyLockoutTimer > 0f)
        {
            return;
        }

        isStunned = true;
        stunTimer = Mathf.Max(stunTimer, finalDuration);
        stunReapplyLockoutTimer = Mathf.Max(0f, stunReapplyLockoutSeconds);
        isAttacking = false;
        SetMovement(Vector2.zero);
    }

    public void ApplyKnockBack(Vector2 projectileDirection, float distanceUnits, float durationSeconds)
    {
        float distance = Mathf.Max(0f, distanceUnits);
        if (distance <= 0f)
        {
            return;
        }

        Vector2 direction = projectileDirection.sqrMagnitude <= Mathf.Epsilon
            ? Vector2.right
            : projectileDirection.normalized;

        float duration = Mathf.Max(0.01f, durationSeconds);
        knockBackVelocity = direction * (distance / duration);
        knockBackTimer = duration;
        isKnockedBack = true;
        isAttacking = false;
        SetMovement(Vector2.zero);
    }

    // override animation so attack takes priority
    protected override void UpdateAnimator()
    {
        if (isAttacking) return;

        base.UpdateAnimator();
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 directionVector = DirectionToVector(GetLastDirection());
        Vector2 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Vector2 hitCenter = origin + (directionVector * AttackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitCenter, Mathf.Max(0.05f, meleeBaseHitRadius));
    }

    protected override void Awake()
    {
        base.Awake();
        EquipMeleeWeapon(defaultMeleeDamage, defaultMeleeRange, defaultMeleeHitRadius);
    }

    protected override void FixedUpdate()
    {
        if (isKnockedBack && !IsDead)
        {
            if (rb != null)
            {
                rb.linearVelocity = knockBackVelocity;
            }

            return;
        }

        base.FixedUpdate();
    }

    private int GetActiveBaseDamage()
    {
        return currentWeaponMode == WeaponMode.Ranged ? rangedBaseDamage : meleeBaseDamage;
    }

    private ProjectileSpawner ResolveProjectileSpawner()
    {
        if (projectileSpawner != null)
        {
            return projectileSpawner;
        }

        projectileSpawner = FindFirstObjectByType<ProjectileSpawner>();
        if (projectileSpawner != null)
        {
            return projectileSpawner;
        }

        projectileSpawner = gameObject.AddComponent<ProjectileSpawner>();
        return projectileSpawner;
    }

    private void UpdateCrowdControlTimers()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            stunReapplyLockoutTimer = Mathf.Max(0f, stunReapplyLockoutTimer - Time.deltaTime);
            if (stunTimer <= 0f)
            {
                stunTimer = 0f;
                isStunned = false;
                stunReapplyLockoutTimer = 0f;
            }
        }
        else
        {
            stunReapplyLockoutTimer = 0f;
        }

        if (isKnockedBack)
        {
            knockBackTimer -= Time.deltaTime;
            if (knockBackTimer <= 0f)
            {
                knockBackTimer = 0f;
                isKnockedBack = false;
                knockBackVelocity = Vector2.zero;
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }

        debugIsStunned = isStunned;
        debugStunTimer = stunTimer;
        debugIsKnockedBack = isKnockedBack;
        debugKnockBackTimer = knockBackTimer;
    }
}
