using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; }

    
    public Vector2 Movement => _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public bool JumpPressed => _jumpAction?.WasPressedThisFrame() ?? false;
    public bool JumpHeld => _jumpAction?.IsPressed() ?? false;
    public bool JumpReleased => _jumpAction?.WasReleasedThisFrame() ?? false;
    public bool RunHeld => _runAction?.IsPressed() ?? false;
    public bool FireHeld => _fireAction?.IsPressed() ?? false;


   
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _fireAction;
    private PlayerInput _playerInput;

    private void Awake() {
        HandleSingleton();
        InitializeInputSystem();
        
    }

    private void HandleSingleton() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeInputSystem() {
        _playerInput = GetComponent<PlayerInput>();

        try {
            CacheActions();
            ConfigureSystemSettings();
        } catch (System.Exception e) {
            Debug.LogError($"Input System Init Failed: {e.Message}");
        }
    }

    private void CacheActions() {
        _moveAction = _playerInput.actions.FindAction("Move");
        _jumpAction = _playerInput.actions.FindAction("Jump");
        _runAction = _playerInput.actions.FindAction("Run");
        _fireAction = _playerInput.actions.FindAction("Fire");
        
    }

    private void ConfigureSystemSettings() {
        _playerInput.neverAutoSwitchControlSchemes = true;
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(DelayedEnable());
    }

    private IEnumerator DelayedEnable() {
        yield return new WaitForEndOfFrame();
        EnableInputSystem();
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        DisableInputSystem();
    }

    private void EnableInputSystem() {
        try {
            if (_playerInput.actions == null) {
                Debug.LogError("Input Actions asset is missing!");
                return;
            }

            
            _playerInput.actions.Enable();

            
            if (!_playerInput.inputIsActive) {
                _playerInput.ActivateInput();
            }

            
            if (Keyboard.current != null) {
                InputSystem.EnableDevice(Keyboard.current);
            }

            
            _playerInput.SwitchCurrentActionMap("Player");
        } catch (System.Exception e) {
            Debug.LogError($"Input Enable Failed: {e}");
        }
    }

    private void EnableDefaultDevices() {
        
        if (Keyboard.current != null) {
            if (!Keyboard.current.enabled)
                InputSystem.EnableDevice(Keyboard.current);
        } else {
            Debug.LogWarning("Keyboard device not found!");
        }

   
    }

    private void DisableInputSystem() {
        if (_playerInput != null && _playerInput.inputIsActive) {
            _playerInput.DeactivateInput();
        }
        InputSystem.DisableDevice(Keyboard.current);
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        StartCoroutine(HandleSceneTransitionInput());
    }

    public IEnumerator HandleSceneTransitionInput() {
        yield return new WaitForEndOfFrame();
        InputSystem.FlushDisconnectedDevices(); 

        if (_playerInput != null) {
            
            _playerInput.DeactivateInput();
            yield return null; 
            _playerInput.ActivateInput();

           
            var playerMap = _playerInput.actions.FindActionMap("Player");
            if (playerMap != null) {
                playerMap.Disable();
                playerMap.Enable();
                _playerInput.SwitchCurrentActionMap("Player");
            } else {
                Debug.LogError("Player action map missing!");
            }

       
            if (Keyboard.current != null && !Keyboard.current.enabled) {
                InputSystem.EnableDevice(Keyboard.current);
            }
        }

        
    }

}