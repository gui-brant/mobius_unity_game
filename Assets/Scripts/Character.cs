using UnityEngine;

public class Character : MonoBehaviour
{
    public int health = 100;
    public float speed = 3f; 

    protected Rigidbody2D rb;
    protected Vector2 movement;

    private int currentDirection = -1;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        Move();
    }

    protected void Move()
    {
        rb.linearVelocity = movement * speed;
        currentDirection = CalculateDirection(movement);
    }

    protected void SetMovement(Vector2 dir)
    {
        movement = dir.normalized;
    }

    public int GetDirection()
    {
        return currentDirection;
    }

    private int CalculateDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return -1;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        return Mathf.RoundToInt(angle / 45f) % 8;
    }
}