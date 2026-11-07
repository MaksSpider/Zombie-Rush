
using System.Collections.Generic;
using UnityEngine;

public class KeysGuideMenuController : MonoBehaviour
{
    public static KeysGuideMenuController Instance;

    private InputManager _inputManager => InputManager.Instance;

    [SerializeField] private GameObject guideKeyPrefab;

    private Dictionary<string, Sprite> _keysDatabase = new();

    private Dictionary<int, GuideKeyController> _guideKeys = new();

    private bool _visible = false;
    private int _nextIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        _inputManager.InputActions.Menu.OpenKeysGuide.performed += SwitchDisplay;
    }

    private void OnDisable()
    {
        _inputManager.InputActions.Menu.OpenKeysGuide.performed -= SwitchDisplay;
    }

    void Start()
    {
        Sprite[] allKeys = Resources.LoadAll<Sprite>("Keys");

        foreach (Sprite sprite in allKeys)
        {
            _keysDatabase[sprite.name.ToLower()] = sprite;
        }

        Debug.Log($"Loaded {_keysDatabase.Count} keys images.");

        Hide();
    }

    public void SwitchDisplay(UnityEngine.InputSystem.InputAction.CallbackContext ctx = default)
    {
        _visible = !_visible;

        if(_visible)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show(UnityEngine.InputSystem.InputAction.CallbackContext ctx = default)
    {
        foreach(GuideKeyController keyController in _guideKeys.Values)
        {
            keyController.Show();
        }
    }

    public void Hide(UnityEngine.InputSystem.InputAction.CallbackContext ctx = default)
    {
        foreach (GuideKeyController keyController in _guideKeys.Values)
        {
            keyController.Hide();
        }
    }

    public int AddKey(string key, string text, int priority = 0)
    {
        int index = _nextIndex;

        Sprite keySprtie = GetKeySprite(key.ToLower());

        GuideKeyController keyGuideCtr = Instantiate(guideKeyPrefab, gameObject.transform).GetComponent<GuideKeyController>();
        keyGuideCtr.Init(keySprtie, text, priority);

        int slotIndex = _guideKeys.Count;

        foreach (GuideKeyController keyController in _guideKeys.Values)
        {
            if(keyController.priority < priority)
            {
                slotIndex = keyController.gameObject.GetComponent<RectTransform>().GetSiblingIndex();
            }
        }

        keyGuideCtr.gameObject.GetComponent<RectTransform>().SetSiblingIndex(slotIndex);

        if(_visible)
        {
            keyGuideCtr.Show();
        }

        _guideKeys[index] = keyGuideCtr;

        _nextIndex++;

        return index;
    }

    public bool RemoveKey(int index)
    {
        if (!_guideKeys.ContainsKey(index)) return false;

        _guideKeys[index].Remove();
        _guideKeys.Remove(index);

        return true;
    }

    public void ResetKeys()
    {
        foreach (GuideKeyController keyController in _guideKeys.Values)
        {
            keyController.Remove();
        }

        _guideKeys.Clear();
    }

    public Sprite GetKeySprite(string key)
    {
        if(!_keysDatabase.ContainsKey(key)) return null;
        return _keysDatabase[key];
    }
}
