using System.Collections;
using UnityEngine;

public class SaahilBoss : Enemy
{
    [Header("Survival Mode")]
    [SerializeField] private float survivalTime = 20f;

    [Header("Boss Buffs")]
    [SerializeField] private float boostedSpeed = 5.5f;
    [SerializeField] private int boostedDamage = 25;

    private float survivalTimer;

    private MoveScene moveScene;
    private bool transitionTriggered = false;


    private int lockedAttackDirection = 0;

    protected override void Awake()
    {
        base.Awake();
        
        survivalTimer = survivalTime;
        speed = boostedSpeed;
    }

    protected override void Update()
    {
        if (!transitionTriggered)
        {
            survivalTimer -= Time.deltaTime;

            if (survivalTimer <= 0f)
            {
                transitionTriggered = true;
                StartCoroutine(HandleWinTransition());
                return;
            }
        }


        if (!isAttacking && movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            lockedAttackDirection = Mathf.RoundToInt(angle / 45f) % 8;
        }

        base.Update();
    }
    
    public override void TakeDamage(int amount)
    {
        Debug.Log("Boss is invincible!");
    }
    
    public override void Die()
    {
    }
    
    private IEnumerator HandleWinTransition()
    {
        Debug.Log("🕒 Player survived! Switching scene...");

        yield return new WaitForSeconds(1f);

        moveScene = FindFirstObjectByType<MoveScene>();

        if (moveScene == null)
        {
            Debug.LogError("MoveScene NOT FOUND");
            yield break;
        }

        yield return moveScene.StartCoroutine(
            moveScene.TransitionProcess("(PGR) Procedurally generated rooms")
        );
    }

    protected override void UpdateAnimator()
    {
        if (animator == null) return;

        int dir;


        if (isAttacking)
        {
            dir = lockedAttackDirection;
            PlayAnimation("Attack" + dir);
            return;
        }
        if (movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            dir = Mathf.RoundToInt(angle / 45f) % 8;
        }
        else
        {
            dir = GetLastDirection();
        }

        if (isHurt)
        {
            PlayAnimation("Hit" + dir);
            return;
        }

        if (movement == Vector2.zero)
        {
            PlayAnimation("Idle" + dir);
        }
        else
        {
            PlayAnimation("Run" + dir);
        }
    }
}