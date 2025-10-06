using System.Collections.Generic;
using UnityEngine;

public class HPDrainTrigger : MonoBehaviour
{
    [Header("Drain Settings")]
    [SerializeField] private DamageType damageType = DamageType.Incendiary;
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private float damageTickInterval = 0.5f;
    [SerializeField] private bool isActive = true;

    [Header("Effects")]
    [SerializeField] private ParticleSystem hazardParticles;
    [SerializeField] private AudioSource hazardAudio;
    [SerializeField] private Light hazardLight;

    [Header("Debug")]
    [SerializeField] private bool showTriggerBounds = true;

    private HashSet<IDamageable> damageablesInTrigger = new();
    private Coroutine drainCoroutine;
    private Collider triggerCollider;

    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            UpdateHazardEffects();
        }
    }

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
        else
            Debug.LogWarning("HPDrainTrigger: No collider found! Add a collider component.");
    }

    private void Start()
    {
        UpdateHazardEffects();
    }

    private void OnEnable()
    {
        drainCoroutine = StartCoroutine(DrainCoroutine());
    }

    private void OnDisable()
    {
        if (drainCoroutine != null)
        {
            StopCoroutine(drainCoroutine);
            drainCoroutine = null;
        }
        damageablesInTrigger.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && damageable.IsAlive)
        {
            damageablesInTrigger.Add(damageable);
            OnDamageableEnter(damageable, other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && damageablesInTrigger.Contains(damageable))
        {
            damageablesInTrigger.Remove(damageable);
            OnDamageableExit(damageable, other);
        }
    }

    protected virtual void OnDamageableEnter(IDamageable damageable, Collider other)
    {
        // Ñan be overridden for specific hazards
        PlayEnterEffects(damageable, other);
    }

    protected virtual void OnDamageableExit(IDamageable damageable, Collider other)
    {
        // Ñan be overridden for specific hazards
        PlayExitEffects(damageable, other);
    }

    private System.Collections.IEnumerator DrainCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(damageTickInterval);

            if (isActive && damageablesInTrigger.Count > 0)
            {
                ApplyDamageToAllInTrigger();
            }
        }
    }

    protected virtual void ApplyDamageToAllInTrigger()
    {
        float damagePerTick = damagePerSecond * damageTickInterval;

        var damageablesToProcess = new List<IDamageable>(damageablesInTrigger);

        foreach (var damageable in damageablesToProcess)
        {
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(damagePerTick, damageType);
                PlayDamageEffects(damageable);
            }
            else
            {
                damageablesInTrigger.Remove(damageable);
            }
        }
    }

    protected virtual void PlayEnterEffects(IDamageable damageable, Collider other)
    {
        // Example: Increase particle intensity when player enters
        if (hazardParticles != null)
        {
            var emission = hazardParticles.emission;
            emission.rateOverTimeMultiplier *= 2f;
        }

        if (hazardAudio != null && !hazardAudio.isPlaying)
        {
            hazardAudio.Play();
        }
    }

    protected virtual void PlayExitEffects(IDamageable damageable, Collider other)
    {
        // Example: Restore particle intensity when player exits
        if (hazardParticles != null)
        {
            var emission = hazardParticles.emission;
            emission.rateOverTimeMultiplier /= 2f;
        }

        if (hazardAudio != null && damageablesInTrigger.Count == 0)
        {
            hazardAudio.Stop();
        }
    }

    protected virtual void PlayDamageEffects(IDamageable damageable)
    {
        // Ñan be customized per hazard type
        // For example, screen shake for player, particle effects, etc.
    }

    private void UpdateHazardEffects()
    {
        if (hazardParticles != null)
        {
            if (isActive && !hazardParticles.isPlaying)
                hazardParticles.Play();
            else if (!isActive && hazardParticles.isPlaying)
                hazardParticles.Stop();
        }

        if (hazardLight != null)
        {
            hazardLight.enabled = isActive;
        }

        if (!isActive)
        {
            damageablesInTrigger.Clear();
            if (hazardAudio != null && hazardAudio.isPlaying)
            {
                hazardAudio.Stop();
            }
        }
    }

    public void ActivateHazard() => IsActive = true;
    public void DeactivateHazard() => IsActive = false;
    public void ToggleHazard() => IsActive = !IsActive;

    public void SetDamageRate(float newDamagePerSecond)
    {
        damagePerSecond = newDamagePerSecond;
    }

    public void SetDamageType(DamageType newDamageType)
    {
        damageType = newDamageType;
        UpdateVisualsForDamageType();
    }

    protected virtual void UpdateVisualsForDamageType()
    {
        if (hazardParticles != null)
        {
            var main = hazardParticles.main;
            main.startColor = GetColorForDamageType(damageType);
        }

        if (hazardLight != null)
        {
            hazardLight.color = GetColorForDamageType(damageType);
        }
    }

    private Color GetColorForDamageType(DamageType type)
    {
        return type switch
        {
            DamageType.Incendiary => Color.red,
            DamageType.Acid => Color.green,
            DamageType.Electrical => Color.blue,
            DamageType.Explosive => Color.yellow,
            DamageType.Kinetic => Color.gray,
            _ => Color.white
        };
    }
    private void OnDrawGizmos()
    {
        if (!showTriggerBounds) return;

        Gizmos.color = isActive ? GetColorForDamageType(damageType) : Color.gray;

        if (TryGetComponent<Collider>(out var collider))
        {
            if (collider is BoxCollider boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }
            else if (collider is SphereCollider sphereCollider)
            {
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawWireSphere(transform.TransformPoint(sphereCollider.center), sphereCollider.radius * transform.lossyScale.x);
            }
            else if (collider is CapsuleCollider capsuleCollider)
            {
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawWireSphere(transform.TransformPoint(capsuleCollider.center), capsuleCollider.radius * transform.lossyScale.x);
            }
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, $"{damageType}\n{damagePerSecond}/sec");
#endif
    }
}