using UnityEngine;

public class Character : MonoBehaviour, IDamageable, IKillable, IInteractable, IMovementController
{
    public int health = 100;
    public float speed = 3f;

    protected Rigidbody2D rb;
    protected Vector2 movement;
    protected Animator animator;

    private int currentDirection = -1;
    private int lastDirection = 0;
    private string currentAnimation = "";
    protected bool isDead = false;

    public bool IsDead => isDead;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        if (isDead) return;
        UpdateAnimator();
    }

    protected virtual void FixedUpdate()
    {
        if (isDead) return;
        Move();
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;

        health -= amount;

        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    public virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        movement = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // TODO: Implement death animation trigger when available.
        // Example:
        // animator.SetTrigger("Die");

        // TODO: Implement death screen popup animation when UI system is ready.
        // Example:
        // DeathScreenUI.Instance.ShowDeathScreen();

        // Halt the game when character dies.
        Time.timeScale = 0f;
    }

    public virtual void Interact(GameObject interactor)
    {
        // Base Character has no default interaction behavior.
    }

    public virtual void Collect(GameObject collector)
    {
        // Characters are not collectible by default.
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

    void IMovementController.SetMovement(Vector2 direction)
    {
        SetMovement(direction);
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
