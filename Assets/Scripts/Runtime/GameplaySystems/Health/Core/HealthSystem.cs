using UnityEngine;

public class HealthSystem : IDamageable
{
    public float CurrentHealth { get; protected set; }
    public float MaxHealth { get; protected set; }
    public bool IsAlive => CurrentHealth > 0;

    public float CurrentArmor { get; protected set; }
    public float MaxArmor { get; protected set; }
    public DamageResistance ArmorResistance { get; set; }

    public event System.Action<float, DamageType> OnDamageTaken;
    public event System.Action<float> OnHealed;
    public event System.Action OnDeath;
    public event System.Action<float> OnArmorChanged;

    protected IHealthView healthView;

    public HealthSystem(float maxHealth, float maxArmor = 0f, IHealthView view = null)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        MaxArmor = maxArmor;
        CurrentArmor = maxArmor;
        healthView = view;
        ArmorResistance = new DamageResistance();
    }

    public virtual void TakeDamage(float amount, DamageType damageType)
    {
        if (!IsAlive) return;

        float damageAfterArmor = CalculateDamageAfterArmor(amount, damageType);
        float originalHealth = CurrentHealth;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damageAfterArmor);

        OnDamageTaken?.Invoke(damageAfterArmor, damageType);
        healthView?.UpdateHealth(CurrentHealth, MaxHealth, true);

        if (CurrentHealth <= 0 && originalHealth > 0)
        {
            OnDeath?.Invoke();
        }
    }

    public virtual void Heal(float amount)
    {
        if (!IsAlive) return;

        float originalHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        float actualHeal = CurrentHealth - originalHealth;

        if (actualHeal > 0)
        {
            OnHealed?.Invoke(actualHeal);
            healthView?.UpdateHealth(CurrentHealth, MaxHealth, false);
        }
    }

    public virtual void AddArmor(float amount)
    {
        CurrentArmor = Mathf.Min(MaxArmor, CurrentArmor + amount);
        OnArmorChanged?.Invoke(CurrentArmor);
        healthView?.UpdateArmor(CurrentArmor);
    }

    protected virtual float CalculateDamageAfterArmor(float baseDamage, DamageType damageType)
    {
        if (CurrentArmor <= 0) return baseDamage;

        float resistance = ArmorResistance.GetResistance(damageType);
        float armorDamageReduction = baseDamage * resistance;
        float damageToArmor = armorDamageReduction * 0.5f;

        CurrentArmor = Mathf.Max(0, CurrentArmor - damageToArmor);
        OnArmorChanged?.Invoke(CurrentArmor);
        healthView?.UpdateArmor(CurrentArmor);

        return baseDamage - armorDamageReduction;
    }

    public void SetHealthView(IHealthView view)
    {
        healthView = view;
    }
}