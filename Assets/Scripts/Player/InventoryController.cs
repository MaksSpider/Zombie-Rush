using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    public Item item;
    public int amount;

    public SlotItem(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    private InputManager _inputManager;
    private PlayerController _playerController;
    private PlayerCombatController _playerCombatController;
    private InteractionsManager _interactionsManager;
    private UpgradesController _upgradesController;

    [SerializeField] private Sprite _selectedSlotSprite;
    [SerializeField] private Sprite _unselectedSlotSprite;

    [SerializeField] private int _baseSlotsAmount = 3;

    [SerializeField] private ItemsData _itemsData;
    [SerializeField] private WeaponsData _weaponsData;
    [SerializeField] private MeleeWeaponsData _meleeWeaponsData;
    [SerializeField] private HealingItemsData _healingItemsData;

    [SerializeField] private GameObject _dropedItemPrefab;
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _inventoryUI;

    public event Action OnSlotChanged;
    public event Action OnDropItemOnSlot;
    public event Action OnSlotsAmountChange;

    public bool inventoryBlocked = false;

    [NonSerialized] public Dictionary<int, SlotItem> inventory = new Dictionary<int, SlotItem>();
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
        _upgradesController = UpgradesController.Instance;

        _playerController = gameObject.GetComponent<PlayerController>();
        _playerCombatController = gameObject.GetComponent<PlayerCombatController>();
    }

    void Start()
    {
        ChangeSlotAmount(_baseSlotsAmount);

        _upgradesController.OnUpgradesChange += () => ChangeSlotAmount(_baseSlotsAmount + (int)_upgradesController.GetBonusValues("bonusInventorySlot"));

        // items to test
        AddItemById(0, 1);
        AddItemById(1, 1);
        AddItemById(2, 1);
        AddItemById(3, 5);
        AddItemById(4, 5);
        AddItemById(5, 5);
        AddItemById(6, 4);
        AddItemById(7, 4);
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
        UpdateSpinningWheelAniamtions();
    }

    private void OnDestroy()
    {
        if (_upgradesController != null)
        {
            _upgradesController.OnUpgradesChange -= () => ChangeSlotAmount(_baseSlotsAmount + (int)_upgradesController.GetBonusValues("bonusInventorySlot"));
        }
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
                slots[i].slot.SetActive(false);
                Destroy(slots[i].slot);
            }

            slots.Clear();
        }

        for(int i = 0; i < amount; i++)
        {
            GameObject slotObject = Instantiate(_slotPrefab, _inventoryUI);
            slotObject.name = $"slot_{i}";

            Image slotImage = slotObject.transform.Find("SlotImage").GetComponent<Image>();
            Image itemImage = slotObject.transform.Find("ItemImage").GetComponent<Image>();
            Image spinningWheel = slotObject.transform.Find("SpinningWheel").GetComponent<Image>();
            Image spinningWheelBackground = slotObject.transform.Find("SpinningWheelBackground").GetComponent<Image>();

            TextMeshProUGUI amountText = slotObject.transform.Find("AmountText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemNameText = slotObject.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();

            slotImage.sprite = _unselectedSlotSprite;
            itemImage.enabled = false;

            slotObject.GetComponent<InventorySlotTooltip>().slotIndex = i;

            slots.Add(new Slot(slotObject, slotImage, itemImage, spinningWheel, spinningWheelBackground, amountText, itemNameText));
            UpdateSlot(i);
        }

        ChangeSlot(selectedSlot);
        OnSlotsAmountChange?.Invoke();
    }

    private void UpdateSpinningWheelAniamtions()
    {
        if (slots.Count < 1) return;

        foreach(Slot slot in slots)
        {
            slot.UpdateAnimation();
        }
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

    public bool AddItemOnSlot(int slot, Item item, int amount)
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

    public bool AddItem(Item item, int amount)
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

        var result = GetFirstEmptySlot();
        if(!result.found) return false;

        return AddItemOnSlot(result.slot, item, amount);
    }

    public bool AddItemById(int id, int amount)
    {
        foreach (Item item in _itemsData.itemsData)
        {
            if (item.id == id)
            {
                return AddItem(item, amount);
            }
        }

        return false;
    }

    public (bool found, int slot) GetFirstEmptySlot()
    {
        for(int i = 0; i < slots.Count; i++)
        {
            if(!inventory.ContainsKey(i))
            {
                return (true, i);
            }
        }

        return (false, 0);
    }

    public void UseItemInHands()
    {
        if (inventoryBlocked) return;
        UseItemOnSlot(selectedSlot);
    }

    public void UseItemOnSlot(int slot)
    {
        if (!inventory.ContainsKey(slot)) return;

        Item item = inventory[slot].item;
        if (!item.isUsable) return;

        var ammoResult = IsAmmo(item.id);

        if (ammoResult.isAmmo)
        {
            (bool isWeapon, IWeapon weapon) = GetWeaponByItemId(ammoResult.weaponId);
            if(!isWeapon) return;

            if (weapon is Weapon normalWeapon)
            {
                int ammoToAdd = 4 * normalWeapon.magazineSize;

                if (item.usageDuration > 0f)
                {
                    inventoryBlocked = true;
                    slots[slot].SetAnimationDuration(item.usageDuration, () => {
                        _playerCombatController.AddAmmo(weapon.itemId, ammoToAdd);
                        RemoveItemById(item.id, 1);
                        inventoryBlocked = false;
                    });
                }
                else
                {
                    _playerCombatController.AddAmmo(weapon.itemId, ammoToAdd);
                    RemoveItemById(item.id, 1);
                }
            }

            return;
        }

        var healingResult = IsHealing(item.id);

        if (healingResult.isHealing)
        {
            if (item.usageDuration > 0f)
            {
                inventoryBlocked = true;
                slots[slot].SetAnimationDuration(item.usageDuration, () => {
                    _playerController.ChangeHealth(healingResult.healingItem.hpToAdd);

                    if (healingResult.healingItem.duration > 0)
                    {
                        _playerController.AddTemporaryHealing(healingResult.healingItem.duration, healingResult.healingItem.maxHpToAdd, healingResult.healingItem.regenerationToAdd, item.icon);
                    }
                    else
                    {
                        _playerController.ChangeMaxHealth(healingResult.healingItem.maxHpToAdd);
                        _playerController.ChangeRegeneration(healingResult.healingItem.regenerationToAdd);
                    }

                    RemoveItemById(item.id, 1);
                    inventoryBlocked = false;
                });
            }
            else
            {
                _playerController.ChangeHealth(healingResult.healingItem.hpToAdd);

                if (healingResult.healingItem.duration > 0)
                {
                    _playerController.AddTemporaryHealing(healingResult.healingItem.duration, healingResult.healingItem.maxHpToAdd, healingResult.healingItem.regenerationToAdd, item.icon);
                }
                else
                {
                    _playerController.ChangeMaxHealth(healingResult.healingItem.maxHpToAdd);
                    _playerController.ChangeRegeneration(healingResult.healingItem.regenerationToAdd);
                }

                RemoveItemById(item.id, 1);
            }

            return;
        }
    }

    // TODO: rework
    public void UseItem(Item item)
    {
        var ammoResult = IsAmmo(item.id);

        if (ammoResult.isAmmo)
        {
            (bool isWeapon, IWeapon weapon) = GetWeaponByItemId(ammoResult.weaponId);
            if(!isWeapon) return; 

            if (weapon is Weapon normalWeapon)
            {
                int ammoToAdd = 4 * normalWeapon.magazineSize;

                _playerCombatController.AddAmmo(weapon.itemId, ammoToAdd);
                RemoveItemById(item.id, 1);
            }

            return;
        }

        var healingResult = IsHealing(item.id);

        if (healingResult.isHealing)
        {
            _playerController.ChangeHealth(healingResult.healingItem.hpToAdd);

            if(healingResult.healingItem.duration > 0)
            {
                _playerController.AddTemporaryHealing(healingResult.healingItem.duration, healingResult.healingItem.maxHpToAdd, healingResult.healingItem.regenerationToAdd);
            }
            else
            {
                _playerController.ChangeMaxHealth(healingResult.healingItem.maxHpToAdd);
                _playerController.ChangeRegeneration(healingResult.healingItem.regenerationToAdd);
            }


            RemoveItemById(item.id, 1);
            return;
        }
    }

    public bool RemoveItemById(int id, int amount)
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

    public void DropItem(Item item, int amount, Vector2 spawnPosition, Vector2 direction)
    {
        GameObject dropedItem = Instantiate(_dropedItemPrefab, spawnPosition, Quaternion.identity);

        SpriteRenderer dropedItemSpriteRenderer = dropedItem.GetComponent<SpriteRenderer>();
        dropedItemSpriteRenderer.sprite = item.icon;

        Rigidbody2D dropedItemRb = dropedItem.GetComponent<Rigidbody2D>();
        dropedItemRb.linearVelocity = direction * 5f;
        dropedItemRb.linearDamping = 3f;

        if(amount > 1)
        {
            dropedItem.transform.Find("AmountText").GetComponent<TextMeshPro>().text = amount + "";
        }
        else
        {
            dropedItem.transform.Find("AmountText").GetComponent<TextMeshPro>().text = "";
        }

        _interactionsManager.RegisterInteractionOnObject("Pickup " + item.name, 0.5f, dropedItem, new Vector2(0, 0.2f), 1.5f, false, (int interactionId) =>
        {
            if (!inventory.ContainsKey(selectedSlot))
            {
                AddItemOnSlot(selectedSlot, item, amount);
                Destroy(dropedItem);
                _interactionsManager.RemoveInteraction(interactionId);
                return;
            }

            if (AddItem(item, amount))
            {
                Destroy(dropedItem);
                _interactionsManager.RemoveInteraction(interactionId);
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

        int amount = _inputManager.InputActions.Player.DropAll.IsPressed() ? inventory[slot].amount : 1;

        DropItem(inventory[slot].item, amount, spawnPosition, direction);
        RemoveItemOnSlot(slot, amount);

        OnDropItemOnSlot?.Invoke();
    }

    public (bool empty, Item item) GetItemOnSlot(int slot)
    {
        if (!inventory.ContainsKey(slot)) return (true, new Item());

        return (false, inventory[slot].item);
    }

    public (bool empty, Item item) GetItemInHands()
    {
        return GetItemOnSlot(selectedSlot);
    }

    public bool IsWeapon(int id)
    {
        foreach(Weapon weapon in _weaponsData.weaponsData)
        {
            if(weapon.itemId == id) return true;
        }

        foreach (MeleeWeapon meleeWeapon in _meleeWeaponsData.meleeWeapons)
        {
            if (meleeWeapon.itemId == id) return true;
        }

        return false;
    }

    public (bool isWeapon, IWeapon iWeapon) GetWeaponByItemId(int id)
    {
        foreach (Weapon weapon in _weaponsData.weaponsData)
        {
            if (weapon.itemId == id) return (true, weapon);
        }

        foreach (MeleeWeapon weapon in _meleeWeaponsData.meleeWeapons)
        {
            if (weapon.itemId == id) return (true, weapon);
        }

        return (false, new Weapon());
    }

    public (bool isAmmo, int weaponId) IsAmmo(int id)
    {
        foreach (Weapon weapon in _weaponsData.weaponsData)
        {
            if (weapon.ammoId == id) return (true, weapon.itemId);
        }

        return (false, -1);
    }

    public (bool isHealing, HealingItem healingItem) IsHealing(int id)
    {
        foreach (HealingItem healingItem in _healingItemsData.healingItems)
        {
            if (healingItem.itemId == id) return (true, healingItem);
        }

        return (false, new HealingItem());
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
            slots[slot].slotImage.color = GetTypeColor(inventory[slot].item.type);
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
        slots[slot].slotImage.color = Color.white;
        slots[slot].itemImage.enabled = false;
    }

    public Color GetTypeColor(string type)
    {
        switch (type)
        {
            case "common":
                return Color.white;
            case "rare":
                return new Color(0.25f, 0.55f, 1.0f);
            case "epic":
                return new Color(0.65f, 0.3f, 1.0f);
            case "legendary":
                return new Color(1.0f, 0.65f, 0.1f);
        }

        return Color.white;
    }
}
