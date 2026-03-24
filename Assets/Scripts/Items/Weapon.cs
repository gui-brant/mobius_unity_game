using UnityEngine;

public class Weapon : Item, IEquipable
{
    [SerializeField] private int damageBonus = 5;
    [SerializeField] private float attackRangeBonus = 0.25f;

    protected override void ApplyTo(Michael michael)
    {
        Equip(michael);
    }

    public void Equip(Michael michael)
    {
        if (michael == null) return;

        michael.ModifyAttackDamage(damageBonus);
        michael.ModifyAttackRange(attackRangeBonus);
    }
}
