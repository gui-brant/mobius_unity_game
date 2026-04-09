using UnityEngine;

public class DevilBoss : MonoBehaviour, IDamageable, IKillable, IAttacker
{
    [Header("Stats")]
    public int health = 200;
    private bool attacking = false;
    
    public int AttackDamage { get; set; }
    public float attackCooldown = 3f;
    public float attackRange = 2f; // Large range i think
    public bool IsDead { get { return health <= 0; } }

    [Header("References")]
    public Transform target;
    private Animator _anim;
    private float _cooldownTimer;
    private bool _isDead = false;
    private MoveScene moveScene;
    public IDamageable michael;
    
    private void HandleAttack(int direction)
    {
        Invoke("AttackMichael", 1.6f);

    }

    // Parameterless class to delay damage
    public void AttackMichael()
    {
        if (michael is not null)
        {
            Attack(michael);
        }   
    }
    
    public void Attack(IDamageable damageable)
    {
        if (damageable == null || target == null) 
        {
            attacking = false;
            return;
        }
        
        float currentDistance = Vector2.Distance(transform.position, target.position);
        
        if (currentDistance <= attackRange + 0.5f)
        {
            damageable.TakeDamage(AttackDamage);
            Debug.Log("Hit landed!");
        }
        else
        {
            Debug.Log("Michael dodged the attack!");
        }
        
        attacking = false;
    }

    public void ModifyAttackDamage(int amount)
    {
        AttackDamage =  amount;
    }

    void Awake()
    {
        ModifyAttackDamage(30);
        
        // Save reference to animator
        _anim = GetComponent<Animator>();
        
        // Find Micheal transform
        if (target == null)
            target = Object.FindFirstObjectByType<Michael>()?.transform;
        
        // Find Micheal script
        if (michael == null)
            michael = Object.FindFirstObjectByType<Michael>();
    }

    void Update()
    {
        if (_isDead || target == null) return;

        _cooldownTimer -= Time.deltaTime;
        // Debug.Log(_cooldownTimer);
        
        // Face the player
        float angle = CalculateAngleToPlayer();
        int direction = Get8DirectionIndex(angle);
        // Debug.Log("facing player");

        // Attack logic
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= attackRange && _cooldownTimer <= 0 && !attacking)
        {
            
            StartAttack(direction);
        }
        else if (distance <= attackRange && _cooldownTimer <= 0)
        {
            // Debug.Log("Cannot attack");
            // If not attacking, stay in the correct Idle direction
            _anim.Play("Idle" + direction);
        }
    }

    private void StartAttack(int dir)
    {
        attacking = true;
        _cooldownTimer = attackCooldown;
        _anim.Play("Attack" + dir); 
        Debug.Log(dir + "Attack started");
        HandleAttack(dir);
    }
    
    public void TakeDamage(int amount)
    {
        if (_isDead) return;
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        _isDead = true;
        _anim.Play("Die");
        //3.assign it through the object in scene
        moveScene = FindFirstObjectByType<MoveScene>();
        //4. call it
        moveScene.StartCoroutine(moveScene.MoveToPGR());
        // Debug.Log("The Beast has fallen.");
    }

    // Helper methods
    private float CalculateAngleToPlayer()
    {
        Vector2 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        return angle;
    }
    
    private int Get8DirectionIndex(float angle)
    {
        return Mathf.RoundToInt(angle / 45f) % 8;
    }
    
   
}