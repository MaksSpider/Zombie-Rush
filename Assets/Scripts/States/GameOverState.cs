using UnityEngine;

public class GameOverState : State
{
    private CameraController _cameraController => CameraController.Instance;

    public override void Tick()
    {
        _cameraController.Tick();
    }
}
