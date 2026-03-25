using UnityEngine;

public class ArmorItem : Item, IConsumable, IHasEffect
{
    [SerializeField] private int armorBonus = 1;

    protected override void ApplyTo(Michael michael)
    {
        Consume(michael);
    }

    public void Consume(Michael michael)
    {
        if (michael == null) return;
        ApplyEffect(michael);
    }

    public void ApplyEffect(Michael michael)
    {
        if (michael == null) return;
        michael.AddArmor(armorBonus);
    }
}
