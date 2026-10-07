using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public struct Ammo
{
    public int inMagazine { get; set; }
    public int all {  get; set; }

    public Ammo(int inMagazine, int all)
    {
        this.inMagazine = inMagazine;
        this.all = all;
    }
}

public class PlayerCombatController : MonoBehaviour
{
    public static PlayerCombatController Instance { get; private set; }

    private InventoryController _inventoryController => InventoryController.Instance;
    private UpgradesController _upgradesController => UpgradesController.Instance;
    private PlayerController _playerController => PlayerController.Instance;
    private AttacksManager _attacksManager => AttacksManager.Instance;
    private InputManager _inputManager => InputManager.Instance;

    [SerializeField] private TextMeshProUGUI _ammoTMP;
    [SerializeField] private Image _bulletIcon;
    [SerializeField] private Image _weaponIcon;

    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _maxSpreadAngle = 15f;

    private float _nextAttackTime;

    private Dictionary<int, Ammo> _ammoInventory = new Dictionary<int, Ammo>();
    
    public IWeapon currentWeapon;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _inventoryController.OnSlotChanged += GetNewWeapon;
        _inventoryController.OnDropItemOnSlot += GetNewWeapon;
        GetNewWeapon();
    }

    public void OnEnterState()
    {
        _inputManager.InputActions.Player.Reload.performed += ctx => ReloadWeapon(currentWeapon);
    }

    public void OnExitState()
    {
        _inputManager.InputActions.Player.Reload.performed -= ctx => ReloadWeapon(currentWeapon);
    }

    private void OnDestroy()
    {
        if (_inventoryController != null)
        {
            _inventoryController.OnSlotChanged -= GetNewWeapon;
            _inventoryController.OnDropItemOnSlot -= GetNewWeapon;
        }
    }

    public void Tick()
    {
        if(currentWeapon != null && currentWeapon is MeleeWeapon)
        {
            UpdateMeleeAttackUI();
        }

        if (_inputManager.InputActions.Player.AttackShoot.IsPressed())
        {
            if (currentWeapon == null) return;
            if (_inventoryController.inventoryBlocked) return;
            if (EventSystem.current.IsPointerOverGameObject()) return;

                Attack(currentWeapon);
        }
    }

    private void GetNewWeapon()
    {
        (bool empty, Item item) = _inventoryController.GetItemInHands();
        if(empty)
        {
            currentWeapon = null;
            UpdateUI();
            return;
        }

        if (!_inventoryController.IsWeapon(item.id))
        {
            currentWeapon = null;
        }
        else
        {
            (bool isWeapon, IWeapon weapon) = _inventoryController.GetWeaponByItemId(item.id);
            if (!isWeapon) return;

            currentWeapon = weapon;
            _nextAttackTime = 0f;
        }

        UpdateUI();
    }

    private void Attack(IWeapon weapon)
    {
        if (Time.time < _nextAttackTime) return;
        _nextAttackTime = Time.time + weapon.attackRate;

        Vector2 mouseScreenPos = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();
        Vector3 mouseScreenPos3D = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z));
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos3D);
        mouseWorldPos.z = 0f;

        Rigidbody2D playerRb = _playerController.GetComponent<Rigidbody2D>();
        Vector2 baseDirection = ((Vector2)mouseWorldPos - playerRb.position).normalized;

        if (weapon is MeleeWeapon meleeWeapon)
        {
            float damageMultiplier = 1f + _upgradesController.GetBonusValues("meleeDamage");
            print(damageMultiplier);

            _attacksManager.CreateMeleeAttack(playerRb.position, baseDirection, meleeWeapon);
        }
        else if (weapon is Weapon normalWeapon)
        {
            bool result = RemoveAmmoFromMag(currentWeapon.itemId, 1);
            if (!result) return;

            LayerMask include = LayerMask.GetMask("Player", "Enemy");

            float damageMultiplier = 1f + _upgradesController.GetBonusValues("bulletsDamage");

            _attacksManager.CreateBullet(playerRb.position, baseDirection, normalWeapon, _maxSpreadAngle, new Vector2(0, 0.15f), damageMultiplier, include);
            UpdateUI();
        }
    }

    public bool ReloadWeapon(IWeapon weapon)
    {
        if (weapon is Weapon normalWeapon)
        {
            if (normalWeapon == null) return false;
            if (!_ammoInventory.ContainsKey(normalWeapon.itemId)) return false;

            int amountToReload = Mathf.Min(normalWeapon.magazineSize - _ammoInventory[normalWeapon.itemId].inMagazine, _ammoInventory[normalWeapon.itemId].all);

            _inventoryController.inventoryBlocked = true;
            _inventoryController.slots[_inventoryController.selectedSlot].SetAnimationDuration(amountToReload * normalWeapon.reloadTime, () => {
                Ammo current = _ammoInventory[normalWeapon.itemId];
                current.inMagazine += amountToReload;
                current.all -= amountToReload;
                _ammoInventory[normalWeapon.itemId] = current;

                _inventoryController.inventoryBlocked = false;
                UpdateUI();
            });

            return true;
        }

        return false;
    }

    public (bool found, int inMagazine, int all) GetCurrentAmmo()
    {
        if (currentWeapon == null) return (false, 0, 0);
        if (!_ammoInventory.ContainsKey(currentWeapon.itemId)) return (true, 0, 0);

        return (true, _ammoInventory[currentWeapon.itemId].inMagazine, _ammoInventory[currentWeapon.itemId].all);
    }

    public void AddAmmo(int weaponItemId, int amount)
    {
        if (!_ammoInventory.ContainsKey(weaponItemId))
        {
            _ammoInventory[weaponItemId] = new Ammo(0, amount);
            return;
        }

        Ammo current = _ammoInventory[weaponItemId];
        current.all += amount;
        _ammoInventory[weaponItemId] = current;
    }

    public bool RemoveAmmoFromMag(int weaponItemId, int amount)
    {
        if (!_ammoInventory.ContainsKey(weaponItemId)) return false;
        if (_ammoInventory[weaponItemId].inMagazine < amount) return false;

        Ammo current = _ammoInventory[weaponItemId];
        current.inMagazine -= amount;
        _ammoInventory[weaponItemId] = current;

        return true;
    }

    private void UpdateMeleeAttackUI()
    {

    }

    private void UpdateUI()
    {
        var result = GetCurrentAmmo();

        if (!result.found)
        {
            _ammoTMP.text = "";
            _bulletIcon.enabled = false;
            _weaponIcon.enabled = false;
            return;
        }

        _weaponIcon.enabled = true;

        if (currentWeapon is Weapon weapon)
        {
            _ammoTMP.text = result.inMagazine + "/" + weapon.magazineSize + " <" + result.all + ">";
            _bulletIcon.enabled = true;
            _bulletIcon.sprite = weapon.bulletUiSprite;
            _weaponIcon.sprite = weapon.weaponUiSprite;
        }
        else if (currentWeapon is MeleeWeapon meleeWeapon)
        {
            _ammoTMP.text = "";
            _bulletIcon.enabled = false;
            _weaponIcon.sprite = meleeWeapon.weaponUiSprite;
        }
    }
}
