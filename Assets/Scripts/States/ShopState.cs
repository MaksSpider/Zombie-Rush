using UnityEngine;

[CreateAssetMenu(fileName = "ShopState", menuName = "States/ShopState")]
public class ShopState : State
{
    private KeysGuideMenuController _keysGuideMenuController => KeysGuideMenuController.Instance;
    private CameraController _cameraController => CameraController.Instance;
    private ShopsManager _shopMenuController => ShopsManager.Instance;
    private InputManager _inputManager => InputManager.Instance;

    public override void OnEnterState()
    {
        _inputManager.InputActions.Player.OpenShop.performed += _shopMenuController.Close;
        _inputManager.InputActions.Menu.Escape.performed += _shopMenuController.Close;

        _keysGuideMenuController.AddKey("ESC", "Close Shop", 10);
    }

    public override void OnExitState()
    {
        _inputManager.InputActions.Player.OpenShop.performed -= _shopMenuController.Close;
        _inputManager.InputActions.Menu.Escape.performed -= _shopMenuController.Close;

        _keysGuideMenuController.ResetKeys();
    }

    public override void Tick()
    {
        _cameraController.Tick();
    }
}
