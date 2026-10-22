public class KillOrbController : OrbController
{
    private PlayerController _playerController => PlayerController.Instance;

    protected override void OnInitialized()
    {
        target = _playerController.killsTMP.rectTransform;
    }

    public override void OnCollect()
    {
        _playerController.ChangeKills(1);
        base.OnCollect();
    }
}