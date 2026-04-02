using UnityEngine;

public abstract class Weapon : Item
{
    public override void Collect(GameObject collector)
    {
        if (!collector.TryGetComponent<Michael>(out Michael michael))
        {
            return;
        }

        // Weapons are persistent pickups: they swap Michael's active mode/profile,
        // but do not get consumed or destroyed.
        ApplyWeaponProfile(michael);
    }

    protected sealed override void ApplyTo(Michael michael) { }

    protected abstract void ApplyWeaponProfile(Michael michael);
}
