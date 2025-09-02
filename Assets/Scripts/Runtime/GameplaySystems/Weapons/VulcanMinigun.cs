using UnityEngine;

public class VulcanMinigun : Weapon
{
    [SerializeField] private float spinUpTime = 0.5f;
    [SerializeField] private float spinDownTime = 1.0f;
    [SerializeField] private AudioClip spinUpSound;
    [SerializeField] private AudioClip spinDownSound;
    [SerializeField] private ParticleSystem[] barrelSmokeEffects;

    private float _currentSpinSpeed = 0f;
    private bool _isSpinningUp = false;
    private bool _isSpinningDown = false;
    private AudioSource _spinAudioSource;
    private Transform _barrels;

    public new void Initialize(WeaponData weaponData, Transform muzzle)
    {
        base.Initialize(weaponData, muzzle);

        _spinAudioSource = gameObject.AddComponent<AudioSource>();
        _spinAudioSource.loop = false;
        _spinAudioSource.spatialBlend = 1f;

        _barrels = transform.Find("Barrels");
        if (_barrels == null)
        {
            Debug.LogWarning("Barrels transform not found in minigun model. Creating one.");
            _barrels = new GameObject("Barrels").transform;
            _barrels.SetParent(transform);
            _barrels.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        if (barrelSmokeEffects == null || barrelSmokeEffects.Length == 0)
        {
            barrelSmokeEffects = GetComponentsInChildren<ParticleSystem>();
        }
    }

    private void Update()
    {
        HandleBarrelSpin();
    }

    private void HandleBarrelSpin()
    {
        if (_isSpinningUp)
        {
            _currentSpinSpeed = Mathf.MoveTowards(_currentSpinSpeed, 1f, Time.deltaTime / spinUpTime);

            if (_currentSpinSpeed >= 1f)
            {
                _isSpinningUp = false;
            }
        }
        else if (_isSpinningDown)
        {
            _currentSpinSpeed = Mathf.MoveTowards(_currentSpinSpeed, 0f, Time.deltaTime / spinDownTime);

            if (_currentSpinSpeed <= 0f)
            {
                _isSpinningDown = false;

                if (spinDownSound != null)
                {
                    _spinAudioSource.PlayOneShot(spinDownSound);
                }
            }
        }

        if (_barrels != null)
        {
            _barrels.Rotate(Vector3.forward, _currentSpinSpeed * 1000f * Time.deltaTime);
        }

        if (barrelSmokeEffects != null)
        {
            foreach (ParticleSystem smoke in barrelSmokeEffects)
            {
                var emission = smoke.emission;
                emission.rateOverTime = _currentSpinSpeed * 50f;
            }
        }
    }

    public override bool CanFire()
    {
        return base.CanFire() && _currentSpinSpeed >= 0.9f;
    }

    public void StartSpinUp()
    {
        if (!_isSpinningUp && _currentSpinSpeed < 0.9f)
        {
            _isSpinningUp = true;
            _isSpinningDown = false;

            if (spinUpSound != null)
            {
                _spinAudioSource.PlayOneShot(spinUpSound);
            }
        }
    }

    public void StartSpinDown()
    {
        if (!_isSpinningDown && _currentSpinSpeed > 0.1f)
        {
            _isSpinningDown = true;
            _isSpinningUp = false;
        }
    }

    public override void Fire()
    {
        if (CanFire())
        {
            base.Fire();

            _currentSpinSpeed = 1f;
            _isSpinningUp = false;
            _isSpinningDown = false;
        }
    }
}