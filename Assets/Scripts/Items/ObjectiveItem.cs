using UnityEngine;
using UnityEngine.Events;

public class ObjectiveItem : Item
{
    [SerializeField] private string objectiveId = "objective_item";
    [SerializeField] private UnityEvent onCollected;

    protected override void ApplyTo(Michael michael)
    {
        if (michael == null) return;

        // Ownership is assigned to Michael when touched/collected.
        michael.ReceiveObjectiveItem(objectiveId);
        // Per-instance special behavior can be configured in the Inspector.
        onCollected?.Invoke();
    }
}
