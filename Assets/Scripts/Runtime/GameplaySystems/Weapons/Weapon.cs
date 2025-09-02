using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData data;
    public int currentAmmo;
    public int upgradeLevel = 0;

    private Transform _muzzleTransform;
    private float _nextFireTime;
    private ParticleSystem _muzzleFlash;
    private AudioSource _fireSound;

    public void Initialize(WeaponData weaponData, Transform muzzle)
    {
        data = weaponData;
        _muzzleTransform = muzzle;
        currentAmmo = data.maxAmmo;

        if (data.modelPrefab != null)
        {
            GameObject model = Instantiate(data.modelPrefab, transform);
            model.transform.localPosition = data.equipPosition;
            model.transform.localEulerAngles = data.equipRotation;

            _muzzleFlash = model.GetComponentInChildren<ParticleSystem>();
            _fireSound = model.GetComponent<AudioSource>();
        }
    }

    public virtual bool CanFire()
    {
        return Time.time >= _nextFireTime && currentAmmo > 0;
    }

    public virtual void Fire()
    {
        if (!CanFire()) return;

        float actualDamage = data.damage * GetDamageMultiplier();
        float actualFireRate = data.fireRate * GetFireRateMultiplier();
        float actualAccuracy = data.accuracy * GetAccuracyMultiplier();

        if (data.weaponName.Contains("Vulcan") || data.weaponName.Contains("Minigun"))
        {
            FireMinigun(actualDamage, actualAccuracy);
        }
        else
        {
            FireStandard(actualDamage, actualAccuracy);
        }

        if (_muzzleFlash != null) _muzzleFlash.Play();
        if (_fireSound != null) _fireSound.Play();

        currentAmmo--;
        _nextFireTime = Time.time + 1f / actualFireRate;

        GetComponentInParent<RobotAnimationController>().OnShootStart();
    }

    private void FireMinigun(float damage, float accuracy)
    {
        int raysPerShot = 3;

        for (int i = 0; i < raysPerShot; i++)
        {
            Vector3 spread = CalculateSpread(accuracy);
            Vector3 direction = (_muzzleTransform.forward + spread).normalized;

            if (Physics.Raycast(_muzzleTransform.position, direction, out RaycastHit hit, data.range))
            {
                ProcessHit(hit, damage);

                Debug.DrawLine(_muzzleTransform.position, hit.point, Color.red, 0.1f);
            }
            else
            {
                Debug.DrawRay(_muzzleTransform.position, direction * data.range, Color.yellow, 0.1f);
            }
        }
    }

    private void FireStandard(float damage, float accuracy)
    {
        Vector3 spread = CalculateSpread(accuracy);
        Vector3 direction = (_muzzleTransform.forward + spread).normalized;

        if (Physics.Raycast(_muzzleTransform.position, direction, out RaycastHit hit, data.range))
        {
            ProcessHit(hit, damage);

            Debug.DrawLine(_muzzleTransform.position, hit.point, Color.red, 0.1f);
        }
        else
        {
            Debug.DrawRay(_muzzleTransform.position, direction * data.range, Color.yellow, 0.1f);
        }
    }

    private Vector3 CalculateSpread(float accuracy)
    {
        float spreadFactor = (1f - accuracy) * 0.1f;

        return new Vector3(
            Random.Range(-spreadFactor, spreadFactor),
            Random.Range(-spreadFactor, spreadFactor),
            Random.Range(-spreadFactor, spreadFactor)
        );
    }

    private void ProcessHit(RaycastHit hit, float damage)
    {
        if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, hit.point, hit.normal);
        }

        if (data.impactEffect != null)
        {
            Quaternion effectRotation = Quaternion.LookRotation(hit.normal);
            GameObject effect = Instantiate(data.impactEffect, hit.point, effectRotation);
            effect.transform.SetParent(hit.transform);
            Destroy(effect, 2f);
        }
    }

    public void Reload()
    {
        // Play reload animation
        currentAmmo = data.maxAmmo + GetAmmoIncrease();
    }

    public bool Upgrade()
    {
        if (upgradeLevel >= data.upgrades.Length) return false;

        upgradeLevel++;
        return true;
    }

    public float GetDamageMultiplier()
    {
        float multiplier = 1f;
        for (int i = 0; i < upgradeLevel && i < data.upgrades.Length; i++)
        {
            multiplier *= data.upgrades[i].damageMultiplier;
        }
        return multiplier;
    }

    public float GetFireRateMultiplier()
    {
        float multiplier = 1f;
        for (int i = 0; i < upgradeLevel && i < data.upgrades.Length; i++)
        {
            multiplier *= data.upgrades[i].fireRateMultiplier;
        }
        return multiplier;
    }

    public float GetAccuracyMultiplier()
    {
        float multiplier = 1f;
        for (int i = 0; i < upgradeLevel && i < data.upgrades.Length; i++)
        {
            multiplier *= data.upgrades[i].accuracyMultiplier;
        }
        return multiplier;
    }

    public int GetAmmoIncrease()
    {
        int increase = 0;
        for (int i = 0; i < upgradeLevel && i < data.upgrades.Length; i++)
        {
            increase += data.upgrades[i].ammoIncrease;
        }
        return increase;
    }
}