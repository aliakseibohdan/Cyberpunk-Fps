using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxArmor = 50f;

    [Header("Armor Resistance")]
    [SerializeField]
    private DamageResistance armorResistance = new()
    {
        kineticResistance = 0.3f,
        incendiaryResistance = 0.1f,
        explosiveResistance = 0.5f,
        electricalResistance = 0.8f,
        acidResistance = 0.2f
    };

    [Header("Effects")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private ParticleSystem damageParticles;

    private HealthSystem healthSystem;
    private AudioSource audioSource;

    public bool IsAlive => healthSystem.IsAlive;
    public float CurrentHealth => healthSystem.CurrentHealth;
    public float MaxHealth => healthSystem.MaxHealth;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        healthSystem = new HealthSystem(maxHealth, maxArmor)
        {
            ArmorResistance = armorResistance
        };

        healthSystem.OnDamageTaken += OnDamageTaken;
        healthSystem.OnHealed += OnHealed;
        healthSystem.OnDeath += OnDeath;
        healthSystem.OnArmorChanged += OnArmorChanged;
    }

    private void Start()
    {
        if (TryGetComponent<IHealthView>(out var healthView))
        {
            healthSystem.SetHealthView(healthView);
        }
    }

    public void TakeDamage(float amount, DamageType damageType)
    {
        healthSystem.TakeDamage(amount, damageType);
    }

    public void Heal(float amount)
    {
        healthSystem.Heal(amount);
    }

    public void AddArmor(float amount)
    {
        healthSystem.AddArmor(amount);
    }

    protected virtual void OnDamageTaken(float damage, DamageType damageType)
    {
        PlayDamageEffects(damageType);

        if (audioSource && damageSound)
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (damageParticles)
        {
            damageParticles.Play();
        }
    }

    protected virtual void OnHealed(float amount)
    {
        if (audioSource && healSound)
        {
            audioSource.PlayOneShot(healSound);
        }
    }

    protected virtual void OnDeath()
    {
        Debug.Log("Player died!");
        // Handle player death (respawn, game over, etc.)
    }

    protected virtual void OnArmorChanged(float currentArmor)
    {
        // Handle armor change effects
    }

    protected virtual void PlayDamageEffects(DamageType damageType)
    {
        // Override this method to implement specific visual/audio effects for each damage type
        switch (damageType)
        {
            case DamageType.Incendiary:
                // Play fire effects
                break;
            case DamageType.Electrical:
                // Play electrical effects
                break;
            case DamageType.Acid:
                // Play acid effects
                break;
            default:
                // Play kinetic and explosive effects
                break;
        }
    }
}