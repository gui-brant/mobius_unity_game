using UnityEngine;

public class DamageTile : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    private float spawnTime;
    private bool hasDealtDamage = false;

    private void Awake()
    {
        spawnTime = Time.time;
        Destroy(transform.parent.gameObject, 2.7f);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (hasDealtDamage) return;
        if (Time.time < spawnTime + 2f) return;

        GameObject target = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        ITeamMember teamMember = target.GetComponent<ITeamMember>();
        if (teamMember == null || teamMember.Team != CombatTeam.Player) return;

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null) damageable = target.GetComponentInParent<IDamageable>();
        if (damageable == null) damageable = target.GetComponentInChildren<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            hasDealtDamage = true;
        }
    }
}