using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("Melee Profile")]
    [SerializeField] private int baseDamage = 20;
    [SerializeField] private float baseAttackRange = 1.25f;
    [SerializeField] private float baseAttackHitRadius = 0.45f;

    protected override void ApplyWeaponProfile(Michael michael)
    {
        if (michael == null) return;
        michael.EquipMeleeWeapon(baseDamage, baseAttackRange, baseAttackHitRadius);
    }
}
