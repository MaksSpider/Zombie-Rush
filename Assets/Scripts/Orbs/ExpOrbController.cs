using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpOrbController : OrbController
{
    private PlayerController _playerController => PlayerController.Instance;

    public int expierience = 1;

    protected override void OnInitialized()
    {
        target = _playerController.xpProgressBar.GetProgressPosition();
    }

    public override void OnCollect()
    {
        _playerController.ChangeExperience(expierience);
        base.OnCollect();
    }
}