using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemporaryHealing
{
    public float endTime;

    public int additionalMaxHp;
    public int additionalRegeneration;

    public TemporaryHealing(float endTime, int additionalMaxHp, int additionalRegeneration)
    {
        this.endTime = endTime;
        this.additionalMaxHp = additionalMaxHp;
        this.additionalRegeneration = additionalRegeneration;
    }
}

public class PlayerController : DamageHandler
{
    public static PlayerController Instance { get; private set; }

    private GameOverMenuController _gameOverMenuController => GameOverMenuController.Instance;
    private UpgradesController _upgradesController => UpgradesController.Instance;
    private StatusesController _statusesController => StatusesController.Instance;
    private CameraController _cameraController => CameraController.Instance;
    private StatesManager _stationsManager => StatesManager.Instance;
    private AudioManager _audioManager => AudioManager.Instance;
    private InputManager _inputManager => InputManager.Instance;

    private InventoryController _inventoryController;

    public event Action OnLevelUp;

    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] private float _baseSpeed = 5f;
    [SerializeField] private float _sprintBoost = 1.5f;

    [SerializeField] private float _collectExpRadius = 1.5f;

    [SerializeField] private int _baseHealth = 100;
    [SerializeField] private int _baseStamina = 500;
    [SerializeField] private int _baseLevelCost = 50;
    [SerializeField] private float _levelExpCostMultipler = 1.25f;
    [SerializeField] private int _healthRegeneration = 2;
    [SerializeField] private int _baseStaminaRegeneration = 2;

    [SerializeField] public Transform overlay;

    [SerializeField] public TextMeshProUGUI levelTMP;
    [SerializeField] public TextMeshProUGUI xpTMP;
    [SerializeField] public TextMeshProUGUI hpTMP;
    [SerializeField] public TextMeshProUGUI killsTMP;
    [SerializeField] public TextMeshProUGUI staminaTMP;

    [SerializeField] public RectTransform levelXpFillMask;

    [NonSerialized] public Vector3 MovementDirection;
    [NonSerialized] public Vector2 LastMovementDirection = Vector2.down;

    private Coroutine updateXpBarRoutine;

    private Animator animator;
    private Rigidbody2D rb;

    public int MaxHealth;

    public int Money = 0;
    public int Kills = 0;

    public int Stamina = 1000;
    public int MaxStamina;
    public int Score = 0;
    public int Experience = 0;
    public int ExperienceToNextLevel = 0;
    public int Level = 1;

    private bool staminaRegeneration = false;

    private List<TemporaryHealing> temporaryHealing = new List<TemporaryHealing>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _inventoryController = gameObject.GetComponent<InventoryController>();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        rb.position = _spawnPosition;

        ExperienceToNextLevel = _baseLevelCost;

        ChangeMaxHealth(_baseHealth);
        ChangeMaxStamina(_baseStamina);

        SetupUI();
    }

    public void OnEnterState()
    {
        _inventoryController.OnSlotsAmountChange += UpdateXpBar;
    }

    public void OnExitState()
    {
        _inventoryController.OnSlotsAmountChange -= UpdateXpBar;
    }


    public void Tick()
    {
        HandleMovementControlls();
        CollectExperience();
    }

    public void FixedTick(float deltaTime)
    {
        HandleMovement(deltaTime);
    }

    public void Tick1s()
    {
        CheckTemporaryHealing();
        ChangeStamina(_baseStaminaRegeneration);
    }

    public void Tick5s()
    {
        ChangeHealth(_healthRegeneration);
    }

    public override void OnDamageDealt(int damage)
    {
        base.OnDamageDealt(damage);

        _cameraController.Shake(2f);

        hpTMP.text = (int)((float)Health / (float)MaxHealth * 100f) + "%";
    }

    // TODO: implement death mechanic
    public override void OnObjectDestroy()
    {
        // animation

        _audioManager.PlaySFX("lose");
        _stationsManager.SwitchState<GameOverState>();
        _gameOverMenuController.Open(Score);
    }

    public void ChangeExperience(int value)
    {
        Experience += value;
        Score += value;

        if (Experience >= ExperienceToNextLevel)
        {
            Experience = 0;
            Level++;

            float levelCost = _baseLevelCost;

            for (int i = 0; i < Level; i++)
            {
                levelCost *= _levelExpCostMultipler;
            }

            ExperienceToNextLevel = (int)levelCost;

            _audioManager.PlaySFX("levelUp");

            _upgradesController.DisplayUpgradesWindow(Level);
            OnLevelUp?.Invoke();
        }

        levelTMP.text = "LVL " + Level.ToString();
        xpTMP.text = Experience.ToString() + "/" + ExperienceToNextLevel.ToString() + " EXP";

        UpdateXpBar();
    }

    public void ChangeHealth(int value)
    {
        Health += value;

        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }

        if(Health <= 0)
        {
            OnObjectDestroy();
        }

        hpTMP.text = (int)((float)Health / (float)MaxHealth * 100f) + "%";
    }

    public void ChangeMaxHealth(int value)
    {
        MaxHealth += value;

        if (value > 0)
        {
            ChangeHealth(value);
            return;
        }

        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }
    }

    public void ChangeRegeneration(int value)
    {
        _healthRegeneration += value;

        if (_healthRegeneration < 0)
        {
            _healthRegeneration = 0;
        }
    }

    public void AddTemporaryHealing(float duration, int additionalMaxHp, int additionalRegeneration, Sprite icon = null)
    {
        temporaryHealing.Add(new TemporaryHealing(Time.time + duration, additionalMaxHp, additionalRegeneration));

        ChangeMaxHealth(additionalMaxHp);
        ChangeRegeneration(additionalRegeneration);

        if (icon == null) return;

        _statusesController.RegisterStatus(
            icon,
            duration,
            new Color(255, 0, 0)
        );
    }

    public void ChangeMaxStamina(int value)
    {
        MaxStamina += value;
        ChangeStamina(value);
    }

    public void ChangeStamina(int value)
    {
        Stamina += value;

        if (Stamina > MaxStamina)
        {
            Stamina = MaxStamina;
        }

        if (Stamina <= 0)
        {
            Stamina = 0;
            staminaRegeneration = true;
        }

        if (Stamina >= MaxStamina * 0.3f)
        {
            staminaRegeneration = false;
        }

        staminaTMP.text = (int)((float)Stamina / (float)MaxStamina * 100f) + "%";
    }

    public void ChangeKills(int value)
    {
        Kills += value;
        Score += value;
        killsTMP.text = Kills.ToString();
    }

    public void ChangeMoney(int value)
    {
        Money += value;
        Score += value;
    }

    private void HandleMovementControlls()
    {
        Vector2 input = _inputManager.InputActions.Player.Move.ReadValue<Vector2>();

        MovementDirection = new Vector3(input.x, input.y, 0f).normalized;

        if (MovementDirection.sqrMagnitude > 0f)
        {
            LastMovementDirection = MovementDirection;
        }

        animator.SetFloat("DirectionHorizontal", LastMovementDirection.x);

        animator.SetFloat("DirectionVertical", LastMovementDirection.y);
    }

    private void HandleMovement(float deltaTime)
    {
        float speed = _baseSpeed;

        if (Stamina > 0 && !staminaRegeneration && _inputManager.InputActions.Player.Sprint.IsPressed())
        {
            speed *= _sprintBoost;

            animator.SetBool("Sprint", true);

            ChangeStamina(-1);
        }
        else
        {
            animator.SetBool("Sprint", false);
        }

        if (MovementDirection.x != 0 || MovementDirection.y != 0)
        {
            animator.SetBool("Movement", true);
        }
        else
        {
            animator.SetBool("Movement", false);
        }

        rb.MovePosition(
            rb.position +
            (Vector2)MovementDirection *
            speed *
            deltaTime
        );
    }

    private void CheckTemporaryHealing()
    {
        for (int i = temporaryHealing.Count - 1; i >= 0; i--)
        {
            TemporaryHealing tempHealing = temporaryHealing[i];

            if (Time.time > tempHealing.endTime)
            {
                ChangeMaxHealth(-tempHealing.additionalMaxHp);
                ChangeRegeneration(-tempHealing.additionalRegeneration);

                temporaryHealing.RemoveAt(i);
            }
        }
    }

    private void CollectExperience()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            _collectExpRadius
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("ExpOrb"))
            {
                ExpOrbController orb =
                    hit.GetComponent<ExpOrbController>();

                if (orb != null)
                {
                    orb.Collect();
                }
            }
        }
    }

    private void SetupUI()
    {
        levelTMP.text = "LVL " + Level.ToString();
        xpTMP.text = Experience.ToString() + "/" + ExperienceToNextLevel.ToString() + " EXP";
        UpdateXpBar();
    }

    private void UpdateXpBar()
    {
        if (updateXpBarRoutine != null)
            StopCoroutine(updateXpBarRoutine);

        updateXpBarRoutine = StartCoroutine(UpdateXpBarNextFrame());
    }

    private IEnumerator UpdateXpBarNextFrame()
    {
        yield return null;

        RectMask2D rectMask = levelXpFillMask.GetComponent<RectMask2D>();

        float percentage = Mathf.Clamp01((float)Experience / (float)ExperienceToNextLevel);
        float width = levelXpFillMask.rect.width;
        float progress = width * (1f - percentage);

        rectMask.padding = new Vector4(0, 0, progress, 0);

        updateXpBarRoutine = null;
    }
}