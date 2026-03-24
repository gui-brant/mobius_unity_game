using UnityEngine;

public class Character : MonoBehaviour
{
    public int health = 100;
    public float speed = 3f;
    public float damage = 1;

    protected Rigidbody2D rb;
    protected Vector2 movement;
    protected Animator animator;
    

    private int currentDirection = -1;
    private int lastDirection = 0;
    private string currentAnimation = "";

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        UpdateAnimator();
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

    // direction system
    public int GetDirection()
    {
        return currentDirection;
    }
    
    public int GetLastDirection()
    {
        return lastDirection;
    }

    private int CalculateDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return -1;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        return Mathf.RoundToInt(angle / 45f) % 8;
    }
    

    // animation system
    protected virtual void UpdateAnimator()
    {
        if (animator == null) return;

        int direction = GetDirection();

        if (direction != -1)
        {
            lastDirection = direction;
        }

        string animName;

        if (movement == Vector2.zero)
        {
            animName = "Idle" + lastDirection;
        }
        else
        {
            animName = "Run" + direction;
        }

        PlayAnimation(animName);
    }

    protected void PlayAnimation(string animName)
    {
        if (currentAnimation == animName) return;

        animator.Play(animName);
        currentAnimation = animName;
    }
}