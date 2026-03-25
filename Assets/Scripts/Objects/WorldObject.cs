using UnityEngine;

public class WorldObject : MonoBehaviour, IDamageable, IKillable, IInteractable, ITargetable
{
    [Header("Durability")]
    [SerializeField] private int maxHealth = 25;

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

    public void Interact(GameObject interactor)
    {
        if (isDead || interactor == null) return;

        // Route interaction through attacker capability so damage comes from the attacker.
        foreach (MonoBehaviour behaviour in interactor.GetComponents<MonoBehaviour>())
        {
            if (behaviour is IAttacker attacker)
            {
                attacker.Attack(this);
                return;
            }
        }
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
}
