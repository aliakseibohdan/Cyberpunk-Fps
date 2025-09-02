using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName;
    [TextArea] public string description;
    public int weaponID;

    [Header("Visuals")]
    public GameObject modelPrefab;
    public Vector3 equipPosition;
    public Vector3 equipRotation;

    [Header("Ammo")]
    public AmmoType ammoType;
    public int startingAmmo = 100;
    public GameObject impactEffect;

    [Header("Combat Stats")]
    public float damage;
    public float fireRate;
    public int maxAmmo;
    public float reloadTime;
    public float accuracy;
    public float range;

    [Header("Upgrades")]
    public WeaponUpgrade[] upgrades;

    [Header("Unlock Status")]
    public bool unlockedByDefault;
}

[System.Serializable]
public class WeaponUpgrade
{
    public string upgradeName;
    [TextArea] public string upgradeDescription;
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public float accuracyMultiplier = 1f;
    public int ammoIncrease = 0;
    public int cost;
}