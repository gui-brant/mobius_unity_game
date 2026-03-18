using UnityEngine;

public class Michael : Character
{
    protected Animator animator;
    private string currentAnimation = "";
    private int lastDirection = 0;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
        UpdateAnimator();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    // input for wasd (DO NOT USE NEW ANIMATION INPUTS)
    private void HandleInput()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        SetMovement(input);
    }

    // Animation code
    private void UpdateAnimator()
    {
        if (animator == null) return;

        int direction = GetDirection();

        // store last direction for idle
        if (direction != -1)
        {
            lastDirection = direction;
        }

        // decide animation
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

    private void PlayAnimation(string animName)
    {
        if (currentAnimation == animName) return;

        animator.Play(animName);
        currentAnimation = animName;
    }
}