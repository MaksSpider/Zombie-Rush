using TMPro;
using UnityEngine;

public class CashOrbController : OrbController
{
    private PlayerController _playerController => PlayerController.Instance;

    [SerializeField] private TextMeshPro _amountTMP;

    [SerializeField] private int _money = 10;

    public int money
    {
        get => _money;
        set
        {
            _money = value;
            UpdateAmountText();
        }
    }

    protected override void OnInitialized()
    {
        target = _playerController.cashTMP.rectTransform;
        UpdateAmountText();
    }

    private void UpdateAmountText()
    {
        if (_amountTMP == null) return;

        _amountTMP.text = money.ToString();
    }

    public override void OnCollect()
    {
        _playerController.ChangeMoney(money);
        base.OnCollect();
    }
}