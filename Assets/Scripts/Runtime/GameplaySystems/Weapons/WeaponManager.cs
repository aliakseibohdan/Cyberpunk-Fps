using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponParent;
    [SerializeField] private Transform muzzlePosition;
    [SerializeField] private WeaponData[] allWeapons;

    private List<Weapon> _weaponInventory = new();
    private int _currentWeaponIndex = -1;
    private Weapon _currentWeapon;

    private KeyControl[] digitKeys = new KeyControl[]
    {
        Keyboard.current.digit0Key,
        Keyboard.current.digit1Key,
        Keyboard.current.digit2Key,
        Keyboard.current.digit3Key,
        Keyboard.current.digit4Key,
        Keyboard.current.digit5Key,
        Keyboard.current.digit6Key,
        Keyboard.current.digit7Key,
        Keyboard.current.digit8Key,
        Keyboard.current.digit9Key,
    };

    private Dictionary<AmmoType, int> _ammoInventory = new();

    private void Start()
    {
        InitializeWeapons();
        InitializeAmmo();
    }

    private void InitializeAmmo()
    {
        foreach (AmmoType type in System.Enum.GetValues(typeof(AmmoType)))
        {
            _ammoInventory[type] = 0;
        }

        WeaponData defaultWeapon = GetDefaultWeapon();
        if (defaultWeapon != null)
        {
            _ammoInventory[defaultWeapon.ammoType] = defaultWeapon.startingAmmo;
        }
    }

    private WeaponData GetDefaultWeapon()
    {
        foreach (WeaponData data in allWeapons)
        {
            if (data.unlockedByDefault)
            {
                return data;
            }
        }
        return null;
    }

    public bool AddAmmo(AmmoType ammoType, int amount)
    {
        if (_ammoInventory.ContainsKey(ammoType))
        {
            _ammoInventory[ammoType] += amount;
            UpdateAmmoUI();
            return true;
        }
        return false;
    }

    public bool HasAmmo(AmmoType ammoType)
    {
        return _ammoInventory.ContainsKey(ammoType) && _ammoInventory[ammoType] > 0;
    }

    public bool UseAmmo(AmmoType ammoType, int amount)
    {
        if (_ammoInventory.ContainsKey(ammoType) && _ammoInventory[ammoType] >= amount)
        {
            _ammoInventory[ammoType] -= amount;
            UpdateAmmoUI();
            return true;
        }
        return false;
    }

    public int GetAmmoCount(AmmoType ammoType)
    {
        return _ammoInventory.ContainsKey(ammoType) ? _ammoInventory[ammoType] : 0;
    }

    private void UpdateAmmoUI()
    {
        if (_currentWeapon != null)
        {
            Debug.Log($"Ammo: {_currentWeapon.currentAmmo} / {GetAmmoCount(_currentWeapon.data.ammoType)}");
        }
    }

    public bool CanFire()
    {
        if (_currentWeapon == null) return false;

        return HasAmmo(_currentWeapon.data.ammoType) || _currentWeapon.currentAmmo > 0;
    }

    public void Fire()
    {
        if (_currentWeapon == null) return;

        if (_currentWeapon.currentAmmo <= 0)
        {
            if (!TryReload()) return;
        }

        _currentWeapon.Fire();
        UpdateAmmoUI();
    }

    public bool TryReload()
    {
        if (_currentWeapon == null) return false;

        int ammoAvailable = GetAmmoCount(_currentWeapon.data.ammoType);
        if (ammoAvailable <= 0) return false;

        int ammoNeeded = _currentWeapon.data.maxAmmo - _currentWeapon.currentAmmo;
        int ammoToUse = Mathf.Min(ammoNeeded, ammoAvailable);

        if (UseAmmo(_currentWeapon.data.ammoType, ammoToUse))
        {
            _currentWeapon.currentAmmo += ammoToUse;
            _currentWeapon.Reload();
            UpdateAmmoUI();
            return true;
        }

        return false;
    }

    private void InitializeWeapons()
    {
        foreach (WeaponData data in allWeapons)
        {
            if (data.unlockedByDefault || PlayerPrefs.GetInt("WeaponUnlocked_" + data.weaponID, 0) == 1)
            {
                AddWeapon(data);
            }
        }

        if (_weaponInventory.Count > 0)
        {
            SwitchWeapon(0);
        }
    }

    private void AddWeapon(WeaponData data)
    {
        GameObject weaponObj = new(data.weaponName);
        weaponObj.transform.SetParent(weaponParent);
        weaponObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        Weapon weapon;

        if (data.weaponName.Contains("Vulcan") || data.weaponName.Contains("Minigun"))
        {
            weapon = weaponObj.AddComponent<VulcanMinigun>();
        }
        else
        {
            weapon = weaponObj.AddComponent<Weapon>();
        }

        weapon.Initialize(data, muzzlePosition);

        _weaponInventory.Add(weapon);

        weaponObj.SetActive(false);
    }

    private void Update()
    {
        HandleWeaponInput();
    }

    private void HandleWeaponInput()
    {
        for (int i = 0; i < 10; i++)
        {
            if (digitKeys[i].wasPressedThisFrame && i < _weaponInventory.Count)
            {
                SwitchWeapon(i);
            }
        }

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0)
        {
            NextWeapon();
        }
        else if (scroll < 0)
        {
            PreviousWeapon();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame && _currentWeapon != null)
        {
            _currentWeapon.Reload();
        }
    }

    public void SwitchWeapon(int index)
    {
        if (index < 0 || index >= _weaponInventory.Count) return;

        if (_currentWeapon != null)
        {
            _currentWeapon.gameObject.SetActive(false);
        }

        _currentWeaponIndex = index;
        _currentWeapon = _weaponInventory[index];
        _currentWeapon.gameObject.SetActive(true);

        UpdateWeaponUI();
    }

    public void NextWeapon()
    {
        int nextIndex = (_currentWeaponIndex + 1) % _weaponInventory.Count;
        SwitchWeapon(nextIndex);
    }

    public void PreviousWeapon()
    {
        int prevIndex = (_currentWeaponIndex - 1 + _weaponInventory.Count) % _weaponInventory.Count;
        SwitchWeapon(prevIndex);
    }

    public void UnlockWeapon(int weaponID)
    {
        foreach (WeaponData data in allWeapons)
        {
            if (data.weaponID == weaponID)
            {
                if (PlayerPrefs.GetInt("WeaponUnlocked_" + weaponID, 0) == 0)
                {
                    PlayerPrefs.SetInt("WeaponUnlocked_" + weaponID, 1);
                    AddWeapon(data);

                    SwitchWeapon(_weaponInventory.Count - 1);
                }
                return;
            }
        }
    }

    public void UpgradeCurrentWeapon()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.Upgrade();
            UpdateWeaponUI();
        }
    }

    private void UpdateWeaponUI()
    {
        // Update UI with current weapon info
    }

    public Weapon GetCurrentWeapon()
    {
        return _currentWeapon;
    }
}