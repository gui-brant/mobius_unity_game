using System.Collections.Generic;
using UnityEngine;

public class SpiritProjectile : MonoBehaviour, IAttacker
{
    // A global list of all spirits currently in the air
    public static List<SpiritProjectile> ActiveSpirits = new List<SpiritProjectile>();

    void OnEnable() => ActiveSpirits.Add(this);
    void OnDisable() => ActiveSpirits.Remove(this);
    
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

        // Simple Homing Logic
        transform.position = Vector3.MoveTowards(
            transform.position, 
            _target.transform.position, 
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
