using UnityEngine;

public abstract class Boss : Enemy
{
    [Header("Boss")]
    [SerializeField] private string bossId = "boss";
    private bool hasTriggeredDeathTransition;

    public virtual string BossId => bossId;
    protected virtual bool ShouldReturnToPgrOnDeath => true;

    public virtual void OnEncounterStart()
    {
    }

    public virtual void OnEncounterEnd()
    {
    }

    public override void Die()
    {
        bool wasDead = IsDead;
        base.Die();

        if (wasDead || !ShouldReturnToPgrOnDeath || hasTriggeredDeathTransition)
        {
            return;
        }

        hasTriggeredDeathTransition = true;

        MoveScene moveScene = FindFirstObjectByType<MoveScene>();
        if (moveScene != null)
        {
            moveScene.StartCoroutine(moveScene.MoveToPGR());
        }
        else
        {
            Debug.LogWarning("MoveScene script not found! Boss death transition to PGR will not run.");
        }
    }
}
