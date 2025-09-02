using UnityEngine;

public enum AmmoType
{
    Light,
    Heavy,
    Explosive,
    Energy,
    Minigun
}

[CreateAssetMenu(fileName = "NewAmmo", menuName = "Weapons/Ammo Data")]
public class AmmoData : ScriptableObject
{
    public AmmoType ammoType;
    public int ammoAmount = 30;
    public GameObject pickupPrefab;
    public AudioClip pickupSound;
    public Color pickupColor = Color.white;
}

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] private AmmoData ammoData;
    [SerializeField] private float rotateSpeed = 50f;
    [SerializeField] private float bounceHeight = 0.5f;
    [SerializeField] private float bounceSpeed = 2f;

    private Vector3 _startPosition;
    private Renderer _renderer;
    private Light _light;

    private void Start()
    {
        _startPosition = transform.position;
        _renderer = GetComponent<Renderer>();
        _light = GetComponentInChildren<Light>();

        if (_renderer != null)
        {
            _renderer.material.color = ammoData.pickupColor;
        }

        if (_light != null)
        {
            _light.color = ammoData.pickupColor;
        }
    }

    private void Update()
    {
        // Rotate and bounce animation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        transform.position = _startPosition + bounceHeight * Mathf.Sin(Time.time * bounceSpeed) * Vector3.up;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponManager weaponManager = other.GetComponentInChildren<WeaponManager>();
            if (weaponManager != null && weaponManager.AddAmmo(ammoData.ammoType, ammoData.ammoAmount))
            {
                if (ammoData.pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(ammoData.pickupSound, transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
}