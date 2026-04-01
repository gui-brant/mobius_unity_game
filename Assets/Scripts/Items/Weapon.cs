using UnityEngine;

public class Weapon : Item
{
    [SerializeField] private int damageBonus = 5;
    [SerializeField] private float attackRangeBonus = 0.25f;

    protected override void ApplyTo(Michael michael)
    {
        if (michael == null) return;

        michael.ModifyAttackDamage(damageBonus);
        michael.ModifyAttackRange(attackRangeBonus);
    }
}
