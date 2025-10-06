public interface IHealthView
{
    void UpdateHealth(float currentHealth, float maxHealth, bool isDamage);
    void UpdateArmor(float currentArmor);
}