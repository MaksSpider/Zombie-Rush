using UnityEngine;

[CreateAssetMenu(fileName = "PauseState", menuName = "States/PauseState")]
public class PauseState : State
{
    private KeysGuideMenuController _keysGuideMenuController => KeysGuideMenuController.Instance;
    private PauseMenuController _pauseMenuController => PauseMenuController.Instance;
    private CameraController _cameraController => CameraController.Instance;
    private InputManager _inputManager => InputManager.Instance;

    public override void OnEnterState()
    {
        _inputManager.InputActions.Menu.Escape.performed += _pauseMenuController.Close;

        _keysGuideMenuController.AddKey("ESC", "Close pause menu", 9);
    }

    public override void OnExitState()
    {
        _inputManager.InputActions.Menu.Escape.performed -= _pauseMenuController.Close;

        _keysGuideMenuController.ResetKeys();
    }

    public override void Tick()
    {
        _cameraController.Tick();
    }
}
