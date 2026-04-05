using UnityEngine;

public class SpiritProjectile : MonoBehaviour, IAttacker
{

    public int AttackDamage { get; private set; } = 20;
    
    [SerializeField] private float speed = 1.8f;
    [SerializeField] private float lifetime = 5f;
    private GameObject _target;
    
    
    public void Setup(GameObject target)
    {
        _target = target;

        // Auto-cleanup if it misses Michael
        Destroy(gameObject, lifetime); 
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_target == null) return;

        // We get the renderer from Michael to find his visual middle point
        Vector3 targetCenter = _target.GetComponent<Renderer>().bounds.center;
        
        // Simple Homing Logic
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetCenter, 
            speed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Try to get Michael's damageable component
        IDamageable damageable = collision.GetComponent<IDamageable>();

        // Check if we hit the Player
        if (damageable != null && collision.CompareTag("Player"))
        {
            Attack(damageable);
            Destroy(gameObject); // Spirit vanishes on impact
        }
    }

    public void Attack(IDamageable target)
        {
            target.TakeDamage(AttackDamage);
        }
    
    public void ModifyAttackDamage(int amount)
    {
        return;
        // Just to satisfy interface
    }
}
