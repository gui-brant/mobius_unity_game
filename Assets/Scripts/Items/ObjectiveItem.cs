using UnityEngine;
using System;

public class ObjectiveItem : Item
{
    public static event Action<ObjectiveItem, Michael> Collected;

    [SerializeField] private string objectiveId = "objective_item";
    protected virtual string ObjectiveId => objectiveId;

    protected override void ApplyTo(Michael michael)
    {
        if (michael == null) return;

        ApplyObjective(michael);
        Collected?.Invoke(this, michael);
    }

    protected virtual void ApplyObjective(Michael michael)
    {
        // Ownership is assigned to Michael when touched/collected.
        michael.ReceiveObjectiveItem(ObjectiveId);
    }
}
