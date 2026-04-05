using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Michael : Character, ITargetable, ITeamMember, IAttacker
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

    private bool isAttacking = false;
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

    private SkullNPC interactableSkullNPC;
    
    protected override void Update()
    {
        if (IsDead) return;

        if (Input.GetKeyDown(KeyCode.X)) TakeDamage(9999); // kys button

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
            float interactRadius = 0.3f; 
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius);

            
            foreach (var col in colliders)
            {
                // Look for the component (Interface)
                IInteractable interactable = col.gameObject.GetComponent<IInteractable>();

                
                if (interactable != null && interactable is Torch)
                {
                    Debug.Log(interactable + "Interactable found");
                    interactable.Interact(this.gameObject);
                    break; // Stop after interacting with the first valid object
                    // Ensuring only one interaction at a time
                }
            }

        }
    }

    // movement 
    private void HandleInput()
    {
        if (isAttacking)
        {
            SetMovement(Vector2.zero);
            return;
        }

        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        SetMovement(input);
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
}
