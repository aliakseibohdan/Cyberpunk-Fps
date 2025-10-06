public interface IDamageable
{
    void TakeDamage(float amount, DamageType damageType);
    void Heal(float amount);
    bool IsAlive { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }
}