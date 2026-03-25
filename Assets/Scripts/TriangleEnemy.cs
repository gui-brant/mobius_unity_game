using UnityEngine;

public class TriangleEnemy : Enemy
{
    TriangleEnemy(Michael michael)
    {
        // Enemy-specific traits
        health = 150;
        speed = 0.3f;

        // Assigning micheal reference
        targetMichael = michael;
        targetTransform = michael.transform;


        // Selecting Movement Strategy
        _movementStrategy = new FollowTargetStrategy(targetTransform);
    }
}
