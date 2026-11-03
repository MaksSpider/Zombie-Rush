using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeState", menuName = "States/UpgradeState")]
public class UpgradeState : State
{
    private KeysGuideMenuController _keysGuideMenuController => KeysGuideMenuController.Instance;
    private UpgradesMenuController _upgradesMenuController => UpgradesMenuController.Instance;
    private CameraController _cameraController => CameraController.Instance;
    private InputManager _inputManager => InputManager.Instance;

    public override void OnEnterState()
    {
        _inputManager.InputActions.Menu.Escape.performed += _upgradesMenuController.CloseUpgradesPicker;

        _keysGuideMenuController.AddKey("MOUSEBUTTONLEFT", "Pick Upgrade", 10);
        _keysGuideMenuController.AddKey("ESC", "Close and dont pick any upgrade", 9);
    }

    public override void OnExitState()
    {
        _inputManager.InputActions.Menu.Escape.performed -= _upgradesMenuController.CloseUpgradesPicker;

        _keysGuideMenuController.ResetKeys();
    }

    public override void Tick()
    {
        _cameraController.Tick();
    }
}
