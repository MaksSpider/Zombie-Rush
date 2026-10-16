using UnityEngine;

[CreateAssetMenu(fileName = "PauseState", menuName = "States/PauseState")]
public class PauseState : State
{
    private PauseMenuController _pauseMenuController => PauseMenuController.Instance;
    private CameraController _cameraController => CameraController.Instance;
    private InputManager _inputManager => InputManager.Instance;

    public override void OnEnterState()
    {
        _inputManager.InputActions.Menu.Escape.performed += _pauseMenuController.Close;
    }

    public override void OnExitState()
    {
        _inputManager.InputActions.Menu.Escape.performed -= _pauseMenuController.Close;
    }

    public override void Tick()
    {
        _cameraController.Tick();
    }
}
