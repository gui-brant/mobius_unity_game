using UnityEngine;

public class Michael : Character
{
    private bool isAttacking = false;

    protected override void Update()
    {
        HandleInput();
        HandleAttack();
        base.Update(); 
    }

    // movement 
    private void HandleInput()
    {
        if (isAttacking)
        {
            SetMovement(Vector2.zero);
            return;
        }

        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        SetMovement(input);
    }

    //attack system
    private void HandleAttack()
    {
        bool holding = Input.GetKey(KeyCode.Space);

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // start attack if not already attacking
        if (holding && !isAttacking)
        {
            StartAttack();
        }

        // check if current animation finished
        if (isAttacking && state.IsName("Attack" + GetLastDirection()) && state.normalizedTime >= 1f)
        {
            isAttacking = false;
        }
    }

    private void StartAttack()
    {
        isAttacking = true;

        int direction = GetDirection();

        // fallback if not moving
        if (direction == -1)
        {
            direction = GetLastDirection();
        }

        string animName = "Attack" + direction;

        PlayAnimation(animName);
    }

    // override animation so attack takes priority
    protected override void UpdateAnimator()
    {
        if (isAttacking) return;

        base.UpdateAnimator();
    }
}