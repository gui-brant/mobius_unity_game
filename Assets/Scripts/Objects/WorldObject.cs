using UnityEngine;

public class WorldObject : MonoBehaviour, IDamageable, IKillable, IInteractable, ITargetable
{
    [Header("Durability")]
    [SerializeField] private int maxHealth = 25;

    [Header("Targeting")]
    [SerializeField] private bool canBeTargeted = true;

    [Header("Drops")]
    [SerializeField] private GameObject objectiveItemPrefab;
    [SerializeField] private float dropYOffset = 0f;

    private int currentHealth;
    private bool isDead;

    public bool IsDead => isDead;
    public Transform TargetTransform => transform;
    public bool CanBeTargeted => canBeTargeted && !isDead;

    public void ConfigureObjectiveDrop(GameObject objectivePrefab)
    {
        if (objectivePrefab == null) return;
        objectiveItemPrefab = objectivePrefab;
    }

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

        SpawnObjectiveDrop();

        Destroy(gameObject);
    }

    private void SpawnObjectiveDrop()
    {
        if (objectiveItemPrefab == null) return;

        Vector3 dropPosition = new Vector3(transform.position.x, transform.position.y + dropYOffset, transform.position.z);
        Transform parent = transform.parent;
        Instantiate(objectiveItemPrefab, dropPosition, Quaternion.identity, parent);
    }
}
