using Unity.VisualScripting;
using UnityEngine;

public class PauseState : State
{
    private PauseMenuController _pauseMenuController;
    private CameraController _cameraController;
    private InputManager _inputManager;

    private void Awake()
    {
        _inputManager = InputManager.Instance;
        _cameraController = CameraController.Instance;
        _pauseMenuController = PauseMenuController.Instance;
    }

    public override void OnEnterState()
    {
        _inputManager.InputActions.Menu.Escape.performed += ctx => _pauseMenuController.Close();
    }

    public override void OnExitState()
    {
        _inputManager.InputActions.Menu.Escape.performed -= ctx => _pauseMenuController.Close();
    }

    public override void Tick()
    {
        _cameraController.Tick();
    }
}
