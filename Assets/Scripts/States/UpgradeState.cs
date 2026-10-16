using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeState", menuName = "States/UpgradeState")]
public class UpgradeState : State
{
    private CameraController _cameraController => CameraController.Instance;

    public override void Tick()
    {
        _cameraController.Tick();
    }
}
