using System.Collections.Generic;
using UnityEngine;

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

    private InputManager _inputManager;
    private InventoryController _inventoryController;
    private PlayerController _playerController;

    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _maxSpreadAngle = 15f;

    private float _nextAttackTime;

    private Dictionary<int, Ammo> _ammoInventory = new Dictionary<int, Ammo>();
    
    public IWeapon currentWeapon;

    private void Start()
    {
        _inventoryController.OnSlotChanged += GetNewWeapon;
        _inventoryController.OnDropItemOnSlot += GetNewWeapon;
        GetNewWeapon();
    }

    private void OnDestroy()
    {
        if (_inventoryController != null)
        {
            _inventoryController.OnSlotChanged -= GetNewWeapon;
            _inventoryController.OnDropItemOnSlot -= GetNewWeapon;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _inputManager = InputManager.Instance;
        _inventoryController = InventoryController.Instance;
        _playerController = PlayerController.Instance;
    }

    public void OnEnterState()
    {
        _inputManager.InputActions.Player.Reload.performed += ctx => ReloadWeapon(currentWeapon);
    }

    public void OnExitState()
    {
        _inputManager.InputActions.Player.Reload.performed -= ctx => ReloadWeapon(currentWeapon);
    }

    public void Tick()
    {
        if (_inputManager.InputActions.Player.AttackShoot.IsPressed())
        {
            if (currentWeapon == null) return;
            if (_inventoryController.inventoryBlocked) return;

            Attack(currentWeapon);
        }
    }

    private void GetNewWeapon()
    {
        Item item = _inventoryController.GetItemInHands();

        if (!_inventoryController.IsWeapon(item.id))
        {
            currentWeapon = null;
            return;
        }

        currentWeapon = _inventoryController.GetWeaponByItemId(item.id);

        _nextAttackTime = 0f;
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
            MeleeAttack(meleeWeapon, baseDirection);
        }
        else if (weapon is Weapon normalWeapon)
        {
            bool result = RemoveAmmoFromMag(currentWeapon.itemId, 1);
            if (!result) return;
            Shoot(normalWeapon, playerRb.position, baseDirection);
        }
    }

    // TODO: implement melee attack logic
    private void MeleeAttack(MeleeWeapon weapon, Vector2 direction)
    {

    }

    private void Shoot(Weapon weapon, Vector2 spawnPosition, Vector2 baseDirection)
    {
        Collider2D playerCollider = GetComponent<Collider2D>();

        for (int i = 0; i < weapon.bulletsPerShot; i++)
        {
            float spread = _maxSpreadAngle * (1f - Mathf.Clamp01(weapon.accuracy));
            float randomAngleOffset = Random.Range(-spread * 0.5f, spread * 0.5f);

            Vector2 direction = Quaternion.Euler(0f, 0f, randomAngleOffset) * baseDirection;

            spawnPosition += direction * weapon.bulletSpawnOffset;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject bulletGameObject = Instantiate(_bulletPrefab, spawnPosition, rotation);

            Rigidbody2D bulletRb = bulletGameObject.GetComponent<Rigidbody2D>();
            bulletRb.linearVelocity = direction * weapon.bulletSpeed;

            SpriteRenderer bulletSpriteRenderer = bulletGameObject.GetComponent<SpriteRenderer>();
            bulletSpriteRenderer.sprite = weapon.bulletSprite;

            Collider2D bulletCollider = bulletGameObject.GetComponent<Collider2D>();
            if (bulletCollider != null && playerCollider != null)
                Physics2D.IgnoreCollision(bulletCollider, playerCollider);

            BulletController bulletCtr = bulletGameObject.GetComponent<BulletController>();
            bulletCtr.damage = weapon.damage;
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
}
