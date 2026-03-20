using UnityEngine;

public class Michael : Character
{
    private bool isAttacking = false;
    private int attackDirection;

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

    // attack system 
    private void HandleAttack()
    {
        bool holding = Input.GetKey(KeyCode.Space);

        // start attack if not attacking and space is held
        if (holding && !isAttacking)
        {
            StartAttack();
        }

        // check if current attack finished
        if (isAttacking)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            if (state.normalizedTime >= 1f)
            {
                isAttacking = false;

                // if still holding space, start next attack
                if (holding)
                {
                    StartAttack();
                }
            }
        }
    }

    private void StartAttack()
    {
        isAttacking = true;

        // get attack direction based on input
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (input != Vector2.zero)
        {
            attackDirection = CalculateDirection(input);
        }
        else
        {
            attackDirection = GetLastDirection();
        }

        string animName = "Attack" + attackDirection;

        PlayAnimation(animName);
    }

    // override animation so attack takes priority
    protected override void UpdateAnimator()
    {
        if (isAttacking) return;

        base.UpdateAnimator();
    }
}