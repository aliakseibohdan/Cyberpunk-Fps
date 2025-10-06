using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [SerializeField] protected float amount = 25f;
    [SerializeField] protected AudioClip pickupSound;
    [SerializeField] protected ParticleSystem pickupEffect;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ApplyPickup(other.gameObject))
            {
                PlayPickupEffects();
                gameObject.SetActive(false);
                Destroy(gameObject, 2f);
            }
        }
    }

    protected abstract bool ApplyPickup(GameObject player);

    protected virtual void PlayPickupEffects()
    {
        if (pickupSound)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        if (pickupEffect)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
    }
}