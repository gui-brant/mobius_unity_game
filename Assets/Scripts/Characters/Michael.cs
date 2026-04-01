using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Michael : Character, ITargetable
{
    
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 1.25f;
    [SerializeField] private float attackHitRadius = 0.45f;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private LayerMask attackableLayers = ~0;
    [SerializeField] private int armor = 0;
    private bool isAttacking = false;
    private readonly HashSet<string> objectiveItems = new HashSet<string>();

    public int AttackDamage => attackDamage;
    public float AttackRange => attackRange;
    public int Armor => armor;
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

        if (interactableSkullNPC != null && Input.GetKeyDown(KeyCode.E))
        {
            interactableSkullNPC.Interact();
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
        yield return new WaitForSeconds(0.35f); // The requested delay
        PerformAttackHit(direction);
    }

    public void Attack(IDamageable target)
    {
        if (isDead || target == null) return;
        target.TakeDamage(AttackDamage);
    }

    private void PerformAttackHit(int direction)
    {
        Vector2 directionVector = DirectionToVector(direction);
        Vector2 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Vector2 hitCenter = origin + (directionVector * attackRange);

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, attackHitRadius, attackableLayers);
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
        attackDamage = Mathf.Max(0, attackDamage + amount);
    }

    public void ModifyAttackRange(float amount)
    {
        attackRange = Mathf.Max(0.1f, attackRange + amount);
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
        Vector2 hitCenter = origin + (directionVector * attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitCenter, attackHitRadius);
    }
}
