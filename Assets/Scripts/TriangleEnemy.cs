using UnityEngine;

public class TriangleEnemy : Enemy
{
    public override void Init(Michael michael)
    {
        // Enemy-specific traits - commented out because they are serialized
        // health = 150;
        /// speed = 0.3f;

        // Assigning Micheal Reference - want to always do
        base.Init(michael);

        // Selecting Movement Strategy
        _movementStrategy = new FollowTargetStrategy(targetTransform);
    }

}
