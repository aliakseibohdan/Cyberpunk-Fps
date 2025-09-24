using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInput _inputActions;
    private InputActionMap _currentMap;

    public PlayerInput.PlayerActions Player => _inputActions.Player;
    public PlayerInput.UIActions UI => _inputActions.UI;

    public InputActionMap CurrentMap { get => _currentMap; set => _currentMap = value; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _inputActions = new PlayerInput();
        EnablePlayerInput();
    }

    public void EnablePlayerInput()
    {
        DisableAllInput();
        _inputActions.Player.Enable();
        _currentMap = _inputActions.Player;
    }

    public void EnableUIInput()
    {
        DisableAllInput();
        _inputActions.UI.Enable();
        _currentMap = _inputActions.UI;
    }

    public void DisableAllInput()
    {
        _inputActions.Player.Disable();
        _inputActions.UI.Disable();
    }

    private void OnDestroy()
    {
        _inputActions?.Dispose();
    }
}