using UnityEngine;

public class HealingItem : Item, IConsumable, IHasEffect
{
    [SerializeField] private int healAmount = 25;

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
        michael.Heal(healAmount);
    }
}
