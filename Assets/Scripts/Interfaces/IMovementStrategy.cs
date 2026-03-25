using UnityEngine;

public interface IMovementStrategy
{
    void Execute(Rigidbody2D rigidbody, float speed);
}
