using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal);
    void OnDestroyed();
}

public class DestructibleObject : MonoBehaviour, IDamageable
{
    [SerializeField] private float health = 100f;
    [SerializeField] private GameObject destroyedVersion;
    [SerializeField] private ParticleSystem hitEffect;

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        health -= damage;

        if (hitEffect != null)
        {
            ParticleSystem effect = Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        if (health <= 0)
        {
            OnDestroyed();
        }
    }

    public void OnDestroyed()
    {
        if (destroyedVersion != null)
        {
            Instantiate(destroyedVersion, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}