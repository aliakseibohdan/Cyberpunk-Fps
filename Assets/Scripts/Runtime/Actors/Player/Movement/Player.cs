using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private WeaponManager weaponManager;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [Space]
    [SerializeField] private Volume volume;
    [SerializeField] private StanceVignette stanceVignette;

    private PlayerInput _inputActions;
    private float _forwardInput;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _inputActions = new PlayerInput();
        _inputActions.Enable();

        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());
        weaponManager = GetComponentInChildren<WeaponManager>();

        cameraSpring.Initialize();
        cameraLean.Initialize();

        stanceVignette.Initialize(volume.profile);
    }

    private void OnDestroy()
    {
        _inputActions.Dispose();
    }

    private void Update()
    {
        var input = _inputActions.Player;
        var deltaTime = Time.deltaTime;

        // Get forward input for camera slide
        var moveInput = input.Move.ReadValue<Vector2>();
        _forwardInput = moveInput.y; // Y axis is forward/backward

        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        playerCamera.UpdateRotation(cameraInput);

        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = moveInput,
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = input.Crouch.WasPressedThisFrame()
                ? CrouchInput.Toggle
                : CrouchInput.None
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);

        if (weaponManager != null)
        {
            Weapon currentWeapon = weaponManager.GetCurrentWeapon();

            VulcanMinigun minigun = currentWeapon as VulcanMinigun;
            if (minigun != null)
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    minigun.StartSpinUp();
                    weaponManager.Fire();
                }
                else if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    minigun.StartSpinDown();

                    GetComponentInChildren<RobotAnimationController>().OnShootEnd();
                }
            }
            else
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    weaponManager.Fire();
                }

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    GetComponentInChildren<RobotAnimationController>().OnShootEnd();
                }
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                weaponManager.TryReload();
            }
        }

#if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                Teleport(hit.point);
            }
        }
#endif
    }

    private void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacter.GetCameraTarget();
        var state = playerCharacter.GetState();

        playerCamera.UpdatePosition(cameraTarget);

        // Pass forward input to the camera spring for slide effect
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up, _forwardInput);

        cameraLean.UpdateLean
        (
            deltaTime,
            state.Stance is Stance.Slide,
            state.Acceleration,
            cameraTarget.up
        );

        stanceVignette.UpdateVignette(deltaTime, state.Stance);
    }

    public void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }
}