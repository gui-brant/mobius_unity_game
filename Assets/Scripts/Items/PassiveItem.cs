using UnityEngine;

public class PassiveItem : Item, IConsumable, IHasEffect
{
    [SerializeField] private int healAmount = 0;
    [SerializeField] private int armorBonus = 0;
    [SerializeField] private int experienceBonus = 0;

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

        if (healAmount > 0)
        {
            michael.Heal(healAmount);
        }

        if (armorBonus != 0)
        {
            michael.AddArmor(armorBonus);
        }

        if (experienceBonus > 0)
        {
            michael.AddExperience(experienceBonus);
        }
    }
}
