using Unity.VisualScripting;
using UnityEngine;

public class UpgradeState : State
{
    private CameraController _cameraController;
    private InputManager _inputManager;

    private void Awake()
    {
        _cameraController = CameraController.Instance;
        _inputManager = InputManager.Instance;
    }

    public override void OnEnterState()
    {
        
    }

    public override void OnExitState()
    {
        
    }

    public override void Tick()
    {
        _cameraController.Tick();
    }
}
