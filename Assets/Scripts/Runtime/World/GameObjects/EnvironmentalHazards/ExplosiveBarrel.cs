using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour, IDamageable
{
    [Header("Barrel Settings")]
    [SerializeField] private float health = 50f;
    [SerializeField] private SphericalExplosion explosionComponent;

    [Header("Visual Feedback")]
    [SerializeField] private ParticleSystem smokeEffect;
    [SerializeField] private Renderer barrelRenderer;
    [SerializeField] private Color damagedColor = Color.red;

    private float currentHealth;
    private Color originalColor;

    public bool IsAlive => currentHealth > 0;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => health;

    private void Awake()
    {
        currentHealth = health;
        explosionComponent = GetComponent<SphericalExplosion>();
        originalColor = barrelRenderer.material.color;
    }

    public void TakeDamage(float amount, DamageType damageType)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        UpdateVisualState();

        PlayDamageEffects(damageType);

        if (currentHealth <= 0)
        {
            Explode();
        }
    }

    public void Heal(float amount)
    {
        // Barrels typically don't heal, but interface requires this
    }

    private void UpdateVisualState()
    {
        float healthPercent = currentHealth / health;
        barrelRenderer.material.color = Color.Lerp(damagedColor, originalColor, healthPercent);

        if (smokeEffect != null)
        {
            var emission = smokeEffect.emission;
            emission.rateOverTimeMultiplier = (1f - healthPercent) * 10f;
        }
    }

    private void PlayDamageEffects(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Incendiary:
                // Chance to instantly explode
                if (Random.value < 0.3f) Explode();
                break;
            case DamageType.Explosive:
                explosionComponent.SetExplosionParameters(
                    explosionComponent.ExplosionRadius * 1.5f,
                    explosionComponent.BaseDamage * 1.2f,
                    DamageType.Explosive
                );
                break;
        }
    }

    private void Explode()
    {
        if (explosionComponent != null)
        {
            explosionComponent.Detonate();
        }
        else
        {
            Debug.LogError("ExplosiveBarrel: No SphericalExplosion component found!");
        }

        gameObject.SetActive(false);
        Destroy(gameObject, 2f);
    }
}