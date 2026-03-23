public interface IAttacker
{
    int AttackDamage { get; }
    void Attack(IDamageable target);
}
