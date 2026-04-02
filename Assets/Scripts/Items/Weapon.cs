using UnityEngine;

public abstract class Weapon : Item
{
    protected sealed override void ApplyTo(Michael michael)
    {
        if (michael == null) return;
        ApplyWeaponProfile(michael);
    }

    protected abstract void ApplyWeaponProfile(Michael michael);
}
