using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene logic

public class Character : MonoBehaviour, IDamageable, IKillable, IInteractable, IMovementController
{
    [Header("Stats")]
    public int health = 100;
    public float speed = 3f;

    [Header("References")]
    private MoveScene sceneController; // Reference to your transition script

    protected Rigidbody2D rb;
    protected Vector2 movement;
    protected Animator animator;

    private int currentDirection = -1;
    private int lastDirection = 0;
    private string currentAnimation = "";
    public bool isDead = false;

    public bool IsDead => isDead;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Automatically find the MoveScene script in the current hierarchy
        sceneController = Object.FindFirstObjectByType<MoveScene>();
        
        if (sceneController == null)
        {
            Debug.LogWarning("MoveScene script not found! Scene transitions on death will not work.");
        }
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
            Spawner.ResetRoomClearProgress();
            sceneController.StartCoroutine(sceneController.MoveBackToSample());
            
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
            rb.bodyType = RigidbodyType2D.Static; // Stop physics
        }

        PlayDeathAnimation();
        Debug.Log($"{gameObject.name} has died and is returning to SampleScene.");
    }

    protected virtual void PlayDeathAnimation()
    {
        if (animator == null) return;
        PlayAnimation("Die");
    }

    public virtual void Interact(GameObject interactor) { }

    public virtual void Collect(GameObject collector) { }

    protected void Move()
    {
        if (rb != null)
        {
            rb.linearVelocity = movement * speed;
            currentDirection = CalculateDirection(movement);
        }
    }

    protected void SetMovement(Vector2 dir)
    {
        movement = dir.normalized;
    }

    void IMovementController.SetMovement(Vector2 direction)
    {
        SetMovement(direction);
    }

    public int GetDirection() => currentDirection;

    public int GetLastDirection() => lastDirection;

    private int CalculateDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return -1;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        return Mathf.RoundToInt(angle / 45f) % 8;
    }

    protected virtual void UpdateAnimator()
    {
        if (animator == null) return;

        int direction = GetDirection();
        if (direction != -1) lastDirection = direction;

        string animName = (movement == Vector2.zero) 
            ? "Idle" + lastDirection 
            : "Run" + direction;

        PlayAnimation(animName);
    }

    protected void PlayAnimation(string animName)
    {
        if (currentAnimation == animName) return;

        animator.Play(animName);
        currentAnimation = animName;
    }
}
