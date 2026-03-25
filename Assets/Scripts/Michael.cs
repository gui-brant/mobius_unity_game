using UnityEngine;

public class Michael : Character
{
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 1.25f;
    [SerializeField] private int armor = 0;
    [SerializeField] private int experience = 0;
    private bool isAttacking = false;

    public override int AttackDamage => attackDamage;
    public float AttackRange => attackRange;
    public int Armor => armor;
    public int Experience => experience;

    protected override void Update()
    {
        if (IsDead) return;
        //DO NOT FORGET TO DELETE THIS LINE BEFORE SUBMIT!!!
        if (Input.GetKeyDown(KeyCode.X)) TakeDamage(9999);

        HandleInput();
        HandleAttack();
        base.Update(); 
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

    // attack system
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

        // fallback if not moving
        if (direction == -1)
        {
            direction = GetLastDirection();
        }

        string animName = "Attack" + direction;

        PlayAnimation(animName);
    }

    public override void Attack(IDamageable target)
    {
        if (isDead || target == null) return;
        target.TakeDamage(AttackDamage);
    }

    public override void TakeDamage(int amount)
    {
        int reducedDamage = Mathf.Max(0, amount - armor);
        if (reducedDamage <= 0) return;

        base.TakeDamage(reducedDamage);
    }

    public override void Die()
    {
        if (IsDead) return;

        // cancel any in-progress attack so the death animation isn't blocked
        isAttacking = false;

        base.Die(); // sets isDead, zeroes movement, plays death animation
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

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        experience += amount;
    }

    // override animation so attack takes priority; death is handled separately in Die()
    protected override void UpdateAnimator()
    {
        if (isAttacking) return;

        base.UpdateAnimator();
    }
}