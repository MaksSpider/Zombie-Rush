using Unity.VisualScripting;
using UnityEngine;

public class UpgradeState : State
{
    private CameraController _cameraController => CameraController.Instance;

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
