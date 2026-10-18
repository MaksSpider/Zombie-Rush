using UnityEngine;

[CreateAssetMenu(fileName = "ShopState", menuName = "States/ShopState")]
public class ShopState : State
{
    private CameraController _cameraController => CameraController.Instance;
    private ShopsManager _shopMenuController => ShopsManager.Instance;
    private InputManager _inputManager => InputManager.Instance;

    public override void OnEnterState()
    {
        _inputManager.InputActions.Menu.Escape.performed += _shopMenuController.Close;
    }

    public override void OnExitState()
    {
        _inputManager.InputActions.Menu.Escape.performed -= _shopMenuController.Close;
    }

    public override void Tick()
    {
        _cameraController.Tick();
    }
}
