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

    private InputManager inputManager;
    private InventoryController inventoryController;
    private PlayerController playerController;

    private float nextAttackTime;

    public IWeapon currentWeapon;

    private float maxSpreadAngle = 15f;
    [SerializeField] private GameObject BulletPrefab;

    private Dictionary<int, Ammo> ammoInventory = new Dictionary<int, Ammo>();

    private void Start()
    {
        inventoryController.OnSlotChanged += GetNewWeapon;
        inventoryController.OnDropItemOnSlot += GetNewWeapon;
        GetNewWeapon();
    }

    private void OnDestroy()
    {
        if (inventoryController != null)
        {
            inventoryController.OnSlotChanged -= GetNewWeapon;
            inventoryController.OnDropItemOnSlot -= GetNewWeapon;
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

        inputManager = InputManager.Instance;
        inventoryController = InventoryController.Instance;
        playerController = PlayerController.Instance;

        inputManager.InputActions.Player.Reload.performed += ctx => ReloadWeapon(currentWeapon);
    }

    void Update()
    {
        if (inputManager.InputActions.Player.AttackShoot.IsPressed())
        {
            if (currentWeapon == null) return;
            if (inventoryController.inventoryBlocked) return;

            Attack(currentWeapon);
        }
    }

    private void GetNewWeapon()
    {
        Item item = inventoryController.GetItemInHands();

        if (!inventoryController.IsWeapon(item.id))
        {
            currentWeapon = null;
            return;
        }

        currentWeapon = inventoryController.GetWeaponByItemId(item.id);

        nextAttackTime = 0f;
    }

    private void Attack(IWeapon weapon)
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + weapon.attackRate;

        Vector2 mouseScreenPos = inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();
        Vector3 mouseScreenPos3D = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z));
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos3D);
        mouseWorldPos.z = 0f;

        Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
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
            float spread = maxSpreadAngle * (1f - Mathf.Clamp01(weapon.accuracy));
            float randomAngleOffset = Random.Range(-spread * 0.5f, spread * 0.5f);

            Vector2 direction = Quaternion.Euler(0f, 0f, randomAngleOffset) * baseDirection;

            spawnPosition += direction * weapon.bulletSpawnOffset;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject bulletGameObject = Instantiate(BulletPrefab, spawnPosition, rotation);

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
            if (!ammoInventory.ContainsKey(normalWeapon.itemId)) return false;

            int amountToReload = Mathf.Min(normalWeapon.magazineSize - ammoInventory[normalWeapon.itemId].inMagazine, ammoInventory[normalWeapon.itemId].all);

            inventoryController.inventoryBlocked = true;
            inventoryController.slots[inventoryController.selectedSlot].SetAnimationDuration(amountToReload * normalWeapon.reloadTime, () => {
                Ammo current = ammoInventory[normalWeapon.itemId];
                current.inMagazine += amountToReload;
                current.all -= amountToReload;
                ammoInventory[normalWeapon.itemId] = current;

                inventoryController.inventoryBlocked = false;
            });

            return true;
        }

        return false;
    }

    public (bool found, int inMagazine, int all) GetCurrentAmmo()
    {
        if (currentWeapon == null) return (false, 0, 0);
        if (!ammoInventory.ContainsKey(currentWeapon.itemId)) return (true, 0, 0);

        return (true, ammoInventory[currentWeapon.itemId].inMagazine, ammoInventory[currentWeapon.itemId].all);
    }

    public void AddAmmo(int weaponItemId, int amount)
    {
        if (!ammoInventory.ContainsKey(weaponItemId))
        {
            ammoInventory[weaponItemId] = new Ammo(0, amount);
            return;
        }

        Ammo current = ammoInventory[weaponItemId];
        current.all += amount;
        ammoInventory[weaponItemId] = current;
    }

    public bool RemoveAmmoFromMag(int weaponItemId, int amount)
    {
        if (!ammoInventory.ContainsKey(weaponItemId)) return false;
        if (ammoInventory[weaponItemId].inMagazine < amount) return false;

        Ammo current = ammoInventory[weaponItemId];
        current.inMagazine -= amount;
        ammoInventory[weaponItemId] = current;

        return true;
    }
}
