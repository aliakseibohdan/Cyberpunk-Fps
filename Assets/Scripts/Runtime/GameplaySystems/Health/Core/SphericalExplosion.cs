using System.Collections.Generic;
using UnityEngine;

public class SphericalExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float baseDamage = 50f;
    [SerializeField] private DamageType damageType = DamageType.Explosive;
    [SerializeField] private LayerMask damageableLayers = Physics.AllLayers;
    [SerializeField] private LayerMask obstacleLayers = Physics.DefaultRaycastLayers;

    [Header("Damage Falloff")]
    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
    [SerializeField] private bool useFalloff = true;

    [Header("Physics Force")]
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;
    [SerializeField] private bool applyForce = true;

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionSoundVolume = 1f;
    [SerializeField] private CameraShakeSettings cameraShake;

    [Header("Timing")]
    [SerializeField] private float explosionDelay = 0f;
    [SerializeField] private bool autoDetonateOnStart = false;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.red;

    private bool hasExploded = false;
    private Collider[] hitColliders = new Collider[50]; // Pre-allocated for performance

    public float ExplosionRadius { get => explosionRadius; set => explosionRadius = value; }
    public float BaseDamage { get => baseDamage; set => baseDamage = value; }

    [System.Serializable]
    public struct CameraShakeSettings
    {
        public float shakeIntensity;
        public float shakeDuration;
        public float shakeDistance;
    }

    private void Start()
    {
        if (autoDetonateOnStart)
        {
            Detonate();
        }
    }

    public void Detonate()
    {
        if (hasExploded) return;

        if (explosionDelay > 0f)
        {
            StartCoroutine(DelayedDetonate());
        }
        else
        {
            ExecuteExplosion();
        }
    }

    public void DetonateWithParameters(float newRadius, float newDamage, DamageType newDamageType)
    {
        explosionRadius = newRadius;
        baseDamage = newDamage;
        damageType = newDamageType;
        Detonate();
    }

    private System.Collections.IEnumerator DelayedDetonate()
    {
        yield return new WaitForSeconds(explosionDelay);
        ExecuteExplosion();
    }

    private void ExecuteExplosion()
    {
        hasExploded = true;

        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, hitColliders, damageableLayers);

        var affectedDamageables = new HashSet<IDamageable>();
        var rigidbodiesToForce = new List<Rigidbody>();

        // First pass: collect all damageables and rigidbodies
        for (int i = 0; i < numColliders; i++)
        {
            var collider = hitColliders[i];
            if (collider == null) continue;

            var damageable = collider.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsAlive && !affectedDamageables.Contains(damageable))
            {
                affectedDamageables.Add(damageable);
            }

            if (applyForce)
            {
                var rb = collider.GetComponent<Rigidbody>();
                if (rb != null && !rigidbodiesToForce.Contains(rb))
                {
                    rigidbodiesToForce.Add(rb);
                }
            }
        }

        // Second pass: apply damage and forces
        foreach (var damageable in affectedDamageables)
        {
            ApplyDamageToTarget(damageable);
        }

        foreach (var rb in rigidbodiesToForce)
        {
            ApplyForceToRigidbody(rb);
        }

        PlayExplosionEffects();

        OnExplosionCompleted();
    }

    private void ApplyDamageToTarget(IDamageable damageable)
    {
        float distance = Vector3.Distance(transform.position, (damageable as MonoBehaviour).transform.position);

        // Check line of sight if obstacle layers are specified
        if (obstacleLayers != 0)
        {
            Vector3 direction = ((damageable as MonoBehaviour).transform.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance, obstacleLayers))
            {
                // Something is blocking the explosion
                if (hit.collider.gameObject != (damageable as MonoBehaviour).gameObject)
                {
                    return; // No damage if blocked
                }
            }
        }

        // Calculate damage with falloff
        float damageMultiplier = useFalloff ? damageFalloff.Evaluate(distance / explosionRadius) : 1f;
        float finalDamage = baseDamage * damageMultiplier;

        damageable.TakeDamage(finalDamage, damageType);

        OnTargetDamaged(damageable, distance, finalDamage);
    }

    private void ApplyForceToRigidbody(Rigidbody rb)
    {
        Vector3 forceDirection = (rb.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, rb.transform.position);
        float forceMultiplier = damageFalloff.Evaluate(distance / explosionRadius);

        rb.AddForce(explosionForce * forceMultiplier * forceDirection, forceMode);
    }

    protected virtual void PlayExplosionEffects()
    {
        if (explosionEffectPrefab != null)
        {
            var effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 5f);
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionSoundVolume);
        }

        if (cameraShake.shakeIntensity > 0f)
        {
            ShakeCamera();
        }

        PlayAdditionalEffects();
    }

    protected virtual void PlayAdditionalEffects()
    {
        // Can be overridden for specific explosion types
        // Example: debris, screen effects, etc.
    }

    protected virtual void OnTargetDamaged(IDamageable damageable, float distance, float damageDealt)
    {
        // Can be overridden for specific behavior when a target is damaged
        // Example: play specific effects, apply status effects, etc.
    }

    protected virtual void OnExplosionCompleted()
    {
        // Can be overridden for cleanup or additional behavior
        // Example: destroy the game object after explosion
        Destroy(gameObject, 0.1f);
    }

    private void ShakeCamera()
    {
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
            if (distanceToCamera <= cameraShake.shakeDistance)
            {
                // Example: mainCamera.GetComponent<CameraShake>()?.Shake(cameraShake.shakeIntensity, cameraShake.shakeDuration);
                Debug.Log($"Camera shake triggered! Intensity: {cameraShake.shakeIntensity}");
            }
        }
    }

    public void SetExplosionParameters(float radius, float damage, DamageType type)
    {
        explosionRadius = radius;
        baseDamage = damage;
        damageType = type;
    }

    public void SetExplosionForce(float force, ForceMode mode = ForceMode.Impulse)
    {
        explosionForce = force;
        forceMode = mode;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

#if UNITY_EDITOR
        if (useFalloff)
        {
            UnityEditor.Handles.color = gizmoColor;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, explosionRadius * 0.25f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, explosionRadius * 0.5f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, explosionRadius * 0.75f);

            // Show damage values at different distances
            UnityEditor.Handles.Label(transform.position + Vector3.up * explosionRadius,
                $"Explosion: {baseDamage} {damageType}\nRadius: {explosionRadius}");
        }
#endif
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // More detailed visualization when selected
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.2f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}