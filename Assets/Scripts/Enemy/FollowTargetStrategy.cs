using UnityEngine;

public class FollowTargetStrategy : IMovementStrategy
{
    // used to reference player's position
    private Transform _target;
    
    // constructor with target as input, allowing for flexibility
    public FollowTargetStrategy(Transform target)
    {
        _target = target;
    }

    public void Execute(Rigidbody2D rigidbody, float speed)
    {
        if (_target == null) return;
        
        // Calculate direction vector for movement
            // Find different between target and movement and convert into unit vector
       Vector2 direction = (_target.position - rigidbody.transform.position).normalized;
       
       // Apply physics to rigidbody
       rigidbody.linearVelocity = direction * speed;
       
       // Handle Animation - animator taken out of method header
       // animator.SetFloat("Speed", rigidbody.linearVelocity.magnitude);
    }
}
