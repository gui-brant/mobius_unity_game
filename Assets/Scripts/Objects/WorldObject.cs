using UnityEngine;

public class WorldObject : MonoBehaviour, IDamageable, IKillable, IInteractable, ITargetable
{
    [Header("Durability")]
    [SerializeField] private int maxHealth = 25;
    [SerializeField] private int impactDamage = 25;

    [Header("Targeting")]
    [SerializeField] private bool canBeTargeted = true;

    private int currentHealth;
    private bool isDead;

    public bool IsDead => isDead;
    public Transform TargetTransform => transform;
    public bool CanBeTargeted => canBeTargeted && !isDead;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandleImpact(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleImpact(other.gameObject);
    }

    public void Interact(GameObject interactor)
    {
        if (isDead) return;
        if (!IsValidImpactor(interactor)) return;

        TakeDamage(impactDamage);
    }

    public void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }

        Destroy(gameObject);
    }

    private void TryHandleImpact(GameObject impactor)
    {
        if (isDead) return;
        if (!IsValidImpactor(impactor)) return;

        Interact(impactor);
    }

    private bool IsValidImpactor(GameObject impactor)
    {
        return impactor.GetComponent<Character>() != null;
    }
}
