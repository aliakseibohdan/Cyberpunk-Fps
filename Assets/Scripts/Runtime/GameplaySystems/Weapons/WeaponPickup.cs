using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private GameObject pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponManager weaponManager = other.GetComponentInChildren<WeaponManager>();
            if (weaponManager != null)
            {
                weaponManager.UnlockWeapon(weaponData.weaponID);

                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, transform.rotation);
                }

                Destroy(gameObject);
            }
        }
    }
}