using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

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
    private AudioManager _audioManager => AudioManager.Instance;

    [SerializeField] private TextMeshProUGUI _ammoTMP;
    [SerializeField] private Image _bulletIcon;
    [SerializeField] private Image _weaponIcon;

    [SerializeField] private GameObject _attackRange;

    private float _nextAttackTime;
    private bool _isReloading = false;

    private Dictionary<string, Ammo> _ammoInventory = new Dictionary<string, Ammo>();
    
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
        if(currentWeapon != null)
        {
            UpdateAttackRange(new Vector2(0, 0.15f));
        }
        else
        {
            _attackRange.SetActive(false);
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
        (bool exist, IItem item) = _inventoryController.GetItemInHands();

        if(!exist)
        {
            currentWeapon = null;
            UpdateUI();
            return;
        }

        (bool isWeapon, IWeapon weapon, _) = _inventoryController.GetWeapon(item.id);
        if(!isWeapon)
        {
            currentWeapon = null;
            UpdateUI();
            return;
        }

        currentWeapon = weapon;
        _nextAttackTime = 0f;
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
            float damageMultiplier = 1f + _playerController.modifiersController.GetModifierValue("melee_damage_multipler_modifier");

            LayerMask include = LayerMask.GetMask("Enemy");

            _attacksManager.CreateMeleeAttack(playerRb.position + new Vector2(0, 0.15f), baseDirection, meleeWeapon, damageMultiplier, include);
        }
        else if (weapon is RangeWeapon normalWeapon)
        {
            bool result = RemoveAmmoFromMag(_inventoryController.GetWeaponItem(currentWeapon).item.id, 1);
            if (!result)
            {
                ReloadWeapon(weapon);
                return;
            }

            LayerMask include = LayerMask.GetMask("Enemy");

            float damageMultiplier = 1f + _playerController.modifiersController.GetModifierValue("bullets_damage_multipler_modifier");

            _attacksManager.CreateBullet(playerRb.position + new Vector2(0, 0.15f), baseDirection, normalWeapon, damageMultiplier, include);

            _audioManager.PlaySFX(normalWeapon.attackSound);
            UpdateUI();
        }
    }

    public bool ReloadWeapon(IWeapon weapon)
    {
        if(_isReloading) return false;

        IItem weaponItem = _inventoryController.GetWeaponItem(currentWeapon).item;

        if (weapon is RangeWeapon normalWeapon)
        {
            if (normalWeapon == null) return false;
            if (!_ammoInventory.ContainsKey(weaponItem.id)) return false;

            int amountToReload = Mathf.Min(normalWeapon.magazineSize - _ammoInventory[weaponItem.id].inMagazine, _ammoInventory[weaponItem.id].all);

            if(amountToReload <= 0) return false;

            _isReloading = true;

            _audioManager.PlaySFX("reload");

            _inventoryController.inventoryBlocked = true;
            _inventoryController.slots[_inventoryController.selectedSlot].SetAnimationDuration(amountToReload * normalWeapon.reloadTime, () => {
                Ammo current = _ammoInventory[weaponItem.id];
                current.inMagazine += amountToReload;
                current.all -= amountToReload;
                _ammoInventory[weaponItem.id] = current;

                _inventoryController.inventoryBlocked = false;
                _isReloading = false;
                UpdateUI();
            });

            return true;
        }

        return false;
    }

    public (bool result, int inMagazine, int all) GetCurrentAmmo()
    {
        if (currentWeapon == null) return (false, 0, 0);

        IItem weaponItem = _inventoryController.GetWeaponItem(currentWeapon).item;
        if (!_ammoInventory.ContainsKey(weaponItem.id)) return (true, 0, 0);

        return (true, _ammoInventory[weaponItem.id].inMagazine, _ammoInventory[weaponItem.id].all);
    }

    public void AddAmmo(string weaponItemId, int amount)
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

    public bool RemoveAmmoFromMag(string weaponItemId, int amount)
    {
        if (!_ammoInventory.ContainsKey(weaponItemId)) return false;
        if (_ammoInventory[weaponItemId].inMagazine < amount) return false;

        Ammo current = _ammoInventory[weaponItemId];
        current.inMagazine -= amount;
        _ammoInventory[weaponItemId] = current;

        return true;
    }

    private void UpdateAttackRange(Vector2 offset)
    {
        if (_attackRange == null || Camera.main == null)
        {
            _attackRange.SetActive(false);
            return;
        }

        _attackRange.SetActive(true);

        Vector2 mouseScreenPos = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();

        Vector3 mouseScreenPos3D = new Vector3(
            mouseScreenPos.x,
            mouseScreenPos.y,
            Mathf.Abs(Camera.main.transform.position.z)
        );

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos3D);
        mouseWorldPos.z = 0f;

        Rigidbody2D playerRb = _playerController.GetComponent<Rigidbody2D>();
        Vector2 playerPosition = playerRb.position + offset;
        Vector2 direction = ((Vector2)mouseWorldPos - playerPosition).normalized;

        _attackRange.transform.position = playerPosition;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _attackRange.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float distanceToMouse = Vector2.Distance((Vector2)mouseWorldPos, playerPosition);

        if(distanceToMouse - 0.25f < currentWeapon.attackRange + 0.25f) distanceToMouse -= 0.25f;

        float maxRadius = Mathf.Max(distanceToMouse, 0.45f);
        float currentRadius = Mathf.Clamp(currentWeapon.attackRange + 0.25f, 0.4f, Mathf.Min(currentWeapon.attackRange + 0.25f, maxRadius));

        DrawSemicircle(currentRadius, 0.25f, currentWeapon.attackAngle);
    }

    private void DrawSemicircle(float radius, float innerRadius, float spreadAngle, int segments = 40)
    {
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * (-spreadAngle / 2f + spreadAngle * i / segments);

            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);

            vertices[i * 2] = new Vector3(
                x * innerRadius,
                y * innerRadius,
                0f
            );

            vertices[i * 2 + 1] = new Vector3(
                x * radius,
                y * radius,
                0f
            );
        }

        for (int i = 0; i < segments; i++)
        {
            int innerCurrent = i * 2;
            int outerCurrent = innerCurrent + 1;
            int innerNext = innerCurrent + 2;
            int outerNext = innerCurrent + 3;

            triangles[i * 6] = innerCurrent;
            triangles[i * 6 + 1] = outerCurrent;
            triangles[i * 6 + 2] = outerNext;

            triangles[i * 6 + 3] = innerCurrent;
            triangles[i * 6 + 4] = outerNext;
            triangles[i * 6 + 5] = innerNext;
        }

        Mesh mesh = _attackRange.GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        _attackRange.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f, 0.27f);
    }

    private void UpdateUI()
    {
        (bool result, int inMagazine, int allAmmo) = GetCurrentAmmo();

        if (!result)
        {
            _ammoTMP.text = "";
            _bulletIcon.enabled = false;
            _weaponIcon.enabled = false;
            return;
        }

        _weaponIcon.enabled = true;

        if (currentWeapon is RangeWeapon weapon)
        {
            _ammoTMP.text = inMagazine + "/" + weapon.magazineSize + " <" + allAmmo + ">";
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
