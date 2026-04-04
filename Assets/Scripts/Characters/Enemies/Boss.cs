using UnityEngine;

public abstract class Boss : Enemy
{
    [Header("Boss")]
    [SerializeField] private string bossId = "boss";

    public virtual string BossId => bossId;

    public virtual void OnEncounterStart()
    {
    }

    public virtual void OnEncounterEnd()
    {
    }
}
