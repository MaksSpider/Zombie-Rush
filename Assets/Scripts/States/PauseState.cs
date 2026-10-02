using Unity.VisualScripting;
using UnityEngine;

public class PauseState : State
{
    private CameraController _cameraController => CameraController.Instance;

    public override void Tick()
    {
        _cameraController.Tick();
    }
}
