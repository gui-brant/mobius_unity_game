using UnityEngine;

public class RangedWeapon : Weapon
{
    [Header("Ranged Profile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectileDamage = 20;

    protected override void ApplyWeaponProfile(Michael michael)
    {
        if (michael == null) return;
        michael.EquipRangedWeapon(projectilePrefab, projectileDamage);
    }
}
