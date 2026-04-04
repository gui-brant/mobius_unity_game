using System;

public class BossObjectiveItem : ObjectiveItem
{
    public static event Action<BossObjectiveItem, Michael> BossCollected;

    protected override void ApplyObjective(Michael michael)
    {
        base.ApplyObjective(michael);
        BossCollected?.Invoke(this, michael);
    }
}
