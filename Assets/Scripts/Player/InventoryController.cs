using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZombieRush.Framework;

public class Slot
{
    public GameObject slot;

    public Image slotImage;
    public Image itemImage;
    private Image _spinningWheel;
    private Image _spinningWheelBackground;

    public TextMeshProUGUI amountText;
    public TextMeshProUGUI itemNameText;

    private float _animationDuration = 0f;
    private float _animationStartTime = 0f;

    private Action _onAnimationComplete;

    public Slot(GameObject slot, Image slotImage, Image itemImage, Image spinningWheel, Image spinningWheelBackground, TextMeshProUGUI amountText, TextMeshProUGUI itemNameText)
    { 
        this.slot = slot;
        this.slotImage = slotImage;
        this.itemImage = itemImage;
        _spinningWheel = spinningWheel;
        _spinningWheelBackground = spinningWheelBackground;
        this.amountText = amountText;
        this.itemNameText = itemNameText;

        _spinningWheel.enabled = false;
        _spinningWheelBackground.enabled = false;

        this.itemNameText.text = string.Empty;
    }

    public void SetAnimationDuration(float duration, Action onComplete = null)
    {
        if (duration <= 0f)
        {
            onComplete.Invoke();
            return;
        }

        _animationDuration = duration;
        _animationStartTime = Time.time;

        _spinningWheel.enabled = true;
        _spinningWheelBackground.enabled = true;
        _spinningWheel.fillAmount = 0f;
        _onAnimationComplete = onComplete;
    }

    public void UpdateAnimation()
    {
        if(_animationDuration == 0f) return;

        if (_animationDuration + _animationStartTime <= Time.time)
        {
            _animationDuration = 0f;
            _animationStartTime = 0f;
            _spinningWheel.enabled = false;
            _spinningWheelBackground.enabled = false;

            _onAnimationComplete?.Invoke();
            _onAnimationComplete = null;

            return;
        }

        float elapsed = Time.time - _animationStartTime;
        _spinningWheel.fillAmount = elapsed / _animationDuration;
        return;
    }
}

public class SlotItem
{
    public IItem item;
    public int amount;

    public SlotItem(IItem item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    private InteractionsManager _interactionsManager;
    private UpgradesController _upgradesController;
    private PlayerController _playerController;
    private InputManager _inputManager;

    [SerializeField] private Sprite _selectedSlotSprite;
    [SerializeField] private Sprite _unselectedSlotSprite;

    [SerializeField] private GameObject _dropedItemPrefab;
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _inventoryUI;

    private Dictionary<string, IItem> _itemsDatabase = new();
    private Dictionary<string, IWeapon> _weaponsDatabase = new();

    private Action _inventorySlotsListener;

    public event Action OnSlotChanged;
    public event Action OnDropItemOnSlot;
    public event Action OnSlotsAmountChange;

    public bool inventoryBlocked = false;

    [NonSerialized] public Dictionary<int, SlotItem> inventory = new();
    [NonSerialized] public List<Slot> slots = new List<Slot>();

    public int selectedSlot = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _inputManager = InputManager.Instance;
        _interactionsManager = InteractionsManager.Instance;

        _playerController = gameObject.GetComponent<PlayerController>();

        _inventorySlotsListener = () =>
        {
            ChangeSlotAmount((int)_playerController.modifiersController.GetModifierValue("base_inventory_slots"));
        };
    }

    void Start()
    {
        Init();
    }

    void OnEnable()
    {
        _playerController.modifiersController.AddOnChangeListener("base_inventory_slots", _inventorySlotsListener);
    }

    void OnDisable()
    {
        _playerController.modifiersController.RemoveOnChangeListener("base_inventory_slots", _inventorySlotsListener);
    }

    public void OnEnterState()
    {
        _inputManager.InputActions.Player.Slot1.performed += ctx => ChangeSlot(0);
        _inputManager.InputActions.Player.Slot2.performed += ctx => ChangeSlot(1);
        _inputManager.InputActions.Player.Slot3.performed += ctx => ChangeSlot(2);
        _inputManager.InputActions.Player.Slot4.performed += ctx => ChangeSlot(3);
        _inputManager.InputActions.Player.Slot5.performed += ctx => ChangeSlot(4);
        _inputManager.InputActions.Player.Slot6.performed += ctx => ChangeSlot(5);
        _inputManager.InputActions.Player.Slot7.performed += ctx => ChangeSlot(6);
        _inputManager.InputActions.Player.Slot8.performed += ctx => ChangeSlot(7);
        _inputManager.InputActions.Player.Slot9.performed += ctx => ChangeSlot(8);

        _inputManager.InputActions.Player.ChangeSlot.performed += OnScrollChangeSlot;

        _inputManager.InputActions.Player.UseItem.performed += ctx => UseItemInHands();
        _inputManager.InputActions.Player.DropItem.performed += ctx => DropItemOnSlot(selectedSlot);
    }

    public void OnExitState()
    {
        _inputManager.InputActions.Player.Slot1.performed -= ctx => ChangeSlot(0);
        _inputManager.InputActions.Player.Slot2.performed -= ctx => ChangeSlot(1);
        _inputManager.InputActions.Player.Slot3.performed -= ctx => ChangeSlot(2);
        _inputManager.InputActions.Player.Slot4.performed -= ctx => ChangeSlot(3);
        _inputManager.InputActions.Player.Slot5.performed -= ctx => ChangeSlot(4);
        _inputManager.InputActions.Player.Slot6.performed -= ctx => ChangeSlot(5);
        _inputManager.InputActions.Player.Slot7.performed -= ctx => ChangeSlot(6);
        _inputManager.InputActions.Player.Slot8.performed -= ctx => ChangeSlot(7);
        _inputManager.InputActions.Player.Slot9.performed -= ctx => ChangeSlot(8);

        _inputManager.InputActions.Player.ChangeSlot.performed -= OnScrollChangeSlot;

        _inputManager.InputActions.Player.UseItem.performed -= ctx => UseItemInHands();
        _inputManager.InputActions.Player.DropItem.performed -= ctx => DropItemOnSlot(selectedSlot);
    }

    public void Tick()
    {
        if (slots.Count < 1) return;

        foreach (Slot slot in slots)
        {
            slot.UpdateAnimation();
        }
    }

    private void OnDestroy()
    {
        if (_upgradesController != null)
        {
            //_upgradesController.OnUpgradesChange -= () => ChangeSlotAmount(_baseSlotsAmount + (int)_upgradesController.GetBonusValues("bonusInventorySlot"));
        }
    }

    private void Init()
    {
        _inventorySlotsListener();

        ScriptableObject[] allItems = Resources.LoadAll<ScriptableObject>("Items");

        foreach (ScriptableObject asset in allItems)
        {
            switch (asset)
            {
                case ItemsData data:
                    RegisterItem(data.item);
                    break;

                case UsableItemData data:
                    RegisterItem(data.usableItem);
                    break;

                case RangeWeaponData data:
                    RegisterItem(data.item);
                    _weaponsDatabase[data.item.id] = data.weapon;
                    break;

                case MeleeWeaponData data:
                    RegisterItem(data.item);
                    _weaponsDatabase[data.item.id] = data.weapon;
                    break;
            }
        }

        Debug.Log($"Loaded {_itemsDatabase.Count} items.");

        // starting items
        AddItem("weapon_pistol", 1);
        AddItem("ammo_pistol", 5);
        AddItem("medkit", 2);
    }

    private void RegisterItem(IItem item)
    {
        _itemsDatabase[item.id] = item;
    }

    private void OnScrollChangeSlot(InputAction.CallbackContext ctx)
    {
        float scrollValue = ctx.ReadValue<float>();

        if (scrollValue < 0)
        {
            ChangeSlot(Mathf.Min(selectedSlot + 1, slots.Count - 1));
        }
        else if (scrollValue > 0)
        {
            ChangeSlot(Mathf.Max(selectedSlot - 1, 0));
        }
    }

    private void GenerateSlots(int amount)
    {
        if(slots.Count > 0)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (inventory.ContainsKey(i) && i >= amount)
                {
                    DropItemOnSlot(i);
                }

                slots[i].slot.GetComponent<Button>().onClick.RemoveAllListeners();
                slots[i].slot.SetActive(false);
                Destroy(slots[i].slot);
            }

            slots.Clear();
        }

        for(int i = 0; i < amount; i++)
        {
            int slotIndex = i;

            GameObject slotObject = Instantiate(_slotPrefab, _inventoryUI);
            slotObject.name = $"slot_{i}";

            slotObject.GetComponent<Button>().onClick.AddListener(() => {
                ChangeSlot(slotIndex);
                print("change");
            });

            Image slotImage = slotObject.transform.Find("SlotImage").GetComponent<Image>();
            Image itemImage = slotObject.transform.Find("ItemImage").GetComponent<Image>();
            Image spinningWheel = slotObject.transform.Find("SpinningWheel").GetComponent<Image>();
            Image spinningWheelBackground = slotObject.transform.Find("SpinningWheelBackground").GetComponent<Image>();

            TextMeshProUGUI amountText = slotObject.transform.Find("AmountText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemNameText = slotObject.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();

            slotImage.sprite = _unselectedSlotSprite;
            itemImage.enabled = false;

            slotObject.GetComponent<InventorySlotTooltip>().slotIndex = slotIndex;

            slots.Add(new Slot(slotObject, slotImage, itemImage, spinningWheel, spinningWheelBackground, amountText, itemNameText));
            UpdateSlot(i);
        }

        if(selectedSlot >= amount) ChangeSlot(0);
        OnSlotsAmountChange?.Invoke();
    }

    public void ChangeSlotAmount(int amount)
    {
        print("Slots amount changed to: " + amount);
        GenerateSlots(amount);
    }

    public void ChangeSlot(int slot)
    {
        if (inventoryBlocked) return;
        if (slot < 0 || slot >= slots.Count) return;


        slots[selectedSlot].slotImage.sprite = _unselectedSlotSprite;
        slots[selectedSlot].itemNameText.text = string.Empty;

        slots[slot].slotImage.sprite = _selectedSlotSprite;
        if (inventory.ContainsKey(slot))
        {
            slots[slot].itemNameText.text = inventory[slot].item.name;
        }
        else
        {
            slots[slot].itemNameText.text = "Empty";
        }

        selectedSlot = slot;

        OnSlotChanged?.Invoke();
    }

    public bool AddItemOnSlot(int slot, IItem item, int amount)
    {
        if (inventory.ContainsKey(slot))
        {
            if (inventory[slot].item.id == item.id && inventory[slot].amount + amount <= item.maxStack)
            {
                inventory[slot].amount += amount;
                UpdateSlot(slot);
                return true;
            }

            return false;
        }

        inventory[slot] = new SlotItem(item, amount);
        UpdateSlot(slot);

        return true;
    }

    public bool AddItem(IItem item, int amount, bool force = false)
    {
        foreach((int slot, SlotItem slotItem) in inventory)
        {
            if(slotItem.item.id == item.id)
            {
                bool isAdded = AddItemOnSlot(slot, item, amount);
                if (isAdded) return true;
                break;
            }
        }

        (bool result, int firstFreeSlot) = GetFirstEmptySlot();
        if (!result && force)
        {
            Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
            Vector2 playerPosition = _playerController.GetComponent<Rigidbody2D>().position;
            Vector2 spawnPosition = playerPosition + direction * 0.5f;

            DropItem(item, amount, spawnPosition, direction);
            return false;
        }

        return AddItemOnSlot(firstFreeSlot, item, amount);
    }

    public bool AddItem(string id, int amount)
    {
        foreach (IItem item in _itemsDatabase.Values)
        {
            if (item.id == id)
            {
                return AddItem(item, amount);
            }
        }

        return false;
    }

    public (bool result, int slot) GetFirstEmptySlot(IItem item = null, int amount = 1)
    {
        for(int slot = 0; slot < slots.Count; slot++)
        {
            if(!inventory.ContainsKey(slot) || item != null && (inventory[slot].item.id == item.id && inventory[slot].amount + amount <= item.maxStack))
            {
                return (true, slot);
            }
        }

        return (false, 0);
    }

    public void UseItemInHands()
    {
        if (inventoryBlocked) return;
        UseItemOnSlot(selectedSlot);
    }

    public bool UseItemOnSlot(int slot)
    {
        if (!inventory.ContainsKey(slot)) return false;

        IItem item = inventory[slot].item;
        
        if (item is UsableItem itemData)
        {
            if (itemData.usageFunction == null)
            {
                Debug.LogError(
                    $"[Inventory] Item '{itemData.id}' has no usageFunction assigned!",
                    this
                );
                return false;
            }

            if (!itemData.usageFunction.CanUseItem(itemData)) return false;

            if (itemData.usageFunction.usageDuration > 0f)
            {
                inventoryBlocked = true;
                slots[slot].SetAnimationDuration(itemData.usageFunction.usageDuration, () =>
                {
                    itemData.usageFunction.UseItem(itemData);
                    if (itemData.usageFunction.remove) RemoveItem(item.id, 1);
                    inventoryBlocked = false;
                });
                return true;
            }

            itemData.usageFunction.UseItem(itemData);
            if(itemData.usageFunction.remove) RemoveItem(item.id, 1);
            return true;
        }

        return false;
    }

    public void UseItem(string id)
    {
        if (!_itemsDatabase.ContainsKey(id)) return;

        IItem item = _itemsDatabase[id];

        if(item is UsableItem itemData)
        {
            if (itemData.usageFunction == null) return;
            itemData.usageFunction.UseItem(itemData);
        }
    }

    public void UseItem(UsableItem item)
    {
        if (item.usageFunction == null) return;
        item.usageFunction.UseItem(item);
    }

    public bool RemoveItem(string id, int amount)
    {
        foreach((int slot, SlotItem slotItem) in inventory)
        {
            if(slotItem.item.id == id)
            {
                return RemoveItemOnSlot(slot, amount);
            }
        }

        return false;
    }

    public bool RemoveItemOnSlot(int slot, int amount)
    {
        if (!inventory.ContainsKey(slot)) return false;

        inventory[slot].amount -= amount;

        if(inventory[slot].amount < 1)
        {
            inventory.Remove(slot);
        }

        UpdateSlot(slot);
        return true;
    }

    public void DropItem(IItem item, int amount, Vector2 spawnPosition, Vector2 direction)
    {
        GameObject dropedItem = Instantiate(_dropedItemPrefab, spawnPosition, Quaternion.identity);

        SpriteRenderer dropedItemSpriteRenderer = dropedItem.GetComponent<SpriteRenderer>();
        dropedItemSpriteRenderer.sprite = item.icon;

        Rigidbody2D dropedItemRb = dropedItem.GetComponent<Rigidbody2D>();
        dropedItemRb.linearVelocity = direction * 3f;
        dropedItemRb.linearDamping = 3f;

        if(amount > 1)
        {
            dropedItem.transform.Find("AmountText").GetComponent<TextMeshPro>().text = amount + "";
        }
        else
        {
            dropedItem.transform.Find("AmountText").GetComponent<TextMeshPro>().text = "";
        }

        _interactionsManager.RegisterInteractionOnObject("Pickup " + item.name, 0.75f, dropedItem, new Vector2(0, 0.2f), 1.5f, true, (int interactionId) =>
        {
            if (!inventory.ContainsKey(selectedSlot))
            {
                AddItemOnSlot(selectedSlot, item, amount);
                Destroy(dropedItem);
                return;
            }

            if (AddItem(item, amount))
            {
                Destroy(dropedItem);
            }
        });
    }

    public void DropItemOnSlot(int slot)
    {
        if (inventoryBlocked) return;
        if (!inventory.ContainsKey(slot)) return;

        Vector2 mouseScreenPos = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();
        Vector3 mouseScreenPos3D = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z));
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos3D);
        mouseWorldPos.z = 0f;

        Vector2 direction = ((Vector2)mouseWorldPos - _playerController.GetComponent<Rigidbody2D>().position).normalized;
        Vector2 spawnPosition = _playerController.GetComponent<Rigidbody2D>().position + direction * 0.5f;

        int amount = _inputManager.InputActions.Player.SelectAll.IsPressed() ? inventory[slot].amount : 1;

        DropItem(inventory[slot].item, amount, spawnPosition, direction);
        RemoveItemOnSlot(slot, amount);

        OnDropItemOnSlot?.Invoke();
    }

    public (bool result, IItem item) GetItem(string id)
    {
        foreach (IItem item in _itemsDatabase.Values)
        {
            if (item.id == id) return (true, item);
        }

        return (false, null);
    }

    public (bool success, IItem item) GetItemOnSlot(int slot)
    {
        if (!inventory.ContainsKey(slot)) return (false, new Item());

        return (true, inventory[slot].item);
    }

    public (bool result, IItem item) GetItemInHands()
    {
        return GetItemOnSlot(selectedSlot);
    }

    public (bool result, IItem item) GetWeaponItem(IWeapon weapon)
    {
        if (weapon == null) return (false, null);

        foreach((string id, IWeapon weaponData) in _weaponsDatabase)
        {
            if (weaponData == weapon && _itemsDatabase.ContainsKey(id)) return (true, _itemsDatabase[id]);
        }

        return (false, null);
    }

    public (bool result, IWeapon iWeapon, IItem item) GetWeapon(string id)
    {
        if(id == null) return (false, null, null);
        if (!_weaponsDatabase.ContainsKey(id) || !_itemsDatabase.ContainsKey(id))
        {
            return (false, null, null);
        }

        return (true, _weaponsDatabase[id], _itemsDatabase[id]);
    }

    public void UpdateSlot(int slot)
    {
        if (slot < 0 || slot >= slots.Count) return;

        if (selectedSlot == slot)
        {
            if (inventory.ContainsKey(slot))
            {
                slots[slot].itemNameText.text = inventory[slot].item.name;
            }
            else
            {
                slots[slot].itemNameText.text = "Empty";
            }
        }
        else
        {
            slots[slot].itemNameText.text = "";
        }

        if (inventory.ContainsKey(slot))
        {
            //slots[slot].slotImage.color = RarityData.GetRarityColor(inventory[slot].item.type);
            slots[slot].itemImage.sprite = inventory[slot].item.icon;
            slots[slot].itemImage.enabled = true;

            if (inventory[slot].item.maxStack != 1)
            {
                slots[slot].amountText.text = inventory[slot].amount + "";
                return;
            }

            slots[slot].amountText.text = "";
            return;
        }

        slots[slot].amountText.text = "";
        slots[slot].slotImage.color = RarityData.GetDefaultRarityColor();
        slots[slot].itemImage.enabled = false;
    }
}
