using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : DamageHandler
{
    public static PlayerController Instance { get; private set; }

    private GameOverMenuController _gameOverMenuController => GameOverMenuController.Instance;
    private CameraController _cameraController => CameraController.Instance;
    private StatesManager _stationsManager => StatesManager.Instance;
    private AudioManager _audioManager => AudioManager.Instance;
    private InputManager _inputManager => InputManager.Instance;
    private GameSession _gameSession => GameSession.Instance;

    private InventoryController _inventoryController;

    [NonSerialized] public ModifiersController modifiersController;
    [NonSerialized] public StatusesController statusesController;
    [NonSerialized] public UpgradesController upgradesController;

    public event Action OnLevelUp;

    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] private float _baseSpeed = 5f;
    [SerializeField] private float _sprintBoost = 1.5f;

    [SerializeField] private float _collectExpRadius = 1.5f;

    [SerializeField] public Transform overlay;

    [SerializeField] public TextMeshProUGUI levelTMP;
    [SerializeField] public TextMeshProUGUI xpTMP;
    [SerializeField] public TextMeshProUGUI hpTMP;
    [SerializeField] public TextMeshProUGUI killsTMP;
    [SerializeField] public TextMeshProUGUI staminaTMP;
    [SerializeField] public TextMeshProUGUI cashTMP;

    [SerializeField] public ProgressBar xpProgressBar;

    [NonSerialized] public Vector3 MovementDirection;
    [NonSerialized] public Vector2 LastMovementDirection = Vector2.down;

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

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _inventoryController = gameObject.GetComponent<InventoryController>();

        modifiersController = GetComponent<ModifiersController>();
        statusesController = GetComponent<StatusesController>();
        upgradesController = GetComponent<UpgradesController>();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        rb.position = _spawnPosition;

        ExperienceToNextLevel = (int)modifiersController.GetModifierValue("base_xp_to_next_level");

        modifiersController.AddOnChangeListener(new List<string>{ "base_max_health", "max_health_multiplier_modifier" }, () =>
        {
            SetMaxHealth((int)(modifiersController.GetModifierValue("base_max_health") * (1f + modifiersController.GetModifierValue("max_health_multiplier_modifier"))));
        });

        modifiersController.AddOnChangeListener(new List<string> { "base_max_stamina", "max_stamina_multiplier_modifier" }, () =>
        {
            SetMaxStamina((int)(modifiersController.GetModifierValue("base_max_stamina") * (1f + modifiersController.GetModifierValue("max_stamina_multiplier_modifier"))));
        });

        modifiersController.AddModifier(new List<ModifierProperty>
        {
            new ModifierProperty
            {
                name = "base_max_health",
                value = 100
            },
            new ModifierProperty
            {
                name = "base_health_regeneration",
                value = 1
            },
            new ModifierProperty
            {
                name = "base_max_stamina",
                value = 500
            },
            new ModifierProperty
            {
                name = "base_xp_to_next_level",
                value = 150
            },
            new ModifierProperty
            {
                name = "level_xp_cost_multipler",
                value = 1.25f
            },
            new ModifierProperty
            {
                name = "base_inventory_slots",
                value = 3
            },
            new ModifierProperty
            {
                name = "base_stamina_regeneration",
                value = 25
            },
        });

        ExperienceToNextLevel = (int)modifiersController.GetModifierValue("base_xp_to_next_level");

        SetupUI();
    }

    public void OnEnterState()
    {
        _inventoryController.OnSlotsAmountChange += () => 
        {
            xpProgressBar.value = (float)Experience / (float)ExperienceToNextLevel;
        };
    }

    public void OnExitState()
    {
        _inventoryController.OnSlotsAmountChange -= () =>
        {
            xpProgressBar.value = (float)Experience / (float)ExperienceToNextLevel;
        };
    }


    public void Tick()
    {
        HandleMovementControlls();
        statusesController.Tick();
        CollectExperience();
    }

    public void FixedTick(float deltaTime)
    {
        HandleMovement(deltaTime);
        statusesController.FixedTick();
    }

    public void Tick1s()
    {
        statusesController.Tick1s();

        float staminaRegeneration = modifiersController.GetModifierValue("base_stamina_regeneration");
        staminaRegeneration *= 1f + modifiersController.GetModifierValue("base_stamina_regeneration_multiplier_modifier");

        ChangeStamina((int)staminaRegeneration);
    }

    public void Tick5s()
    {
        statusesController.Tick5s();

        float healthRegeneration = modifiersController.GetModifierValue("base_health_regeneration");
        healthRegeneration *= 1f + modifiersController.GetModifierValue("health_regeneration_multiplier_modifier");

        ChangeHealth((int)healthRegeneration);
    }

    public void Tick10s()
    {
        statusesController.Tick10s();
    }

    public void Tick30s()
    {
        statusesController.Tick30s();
    }

    public void Tick1m()
    {
        statusesController.Tick1m();
    }

    public override void OnDamageDealt(int damage)
    {
        base.OnDamageDealt(damage);

        _cameraController.Shake(2f);

        hpTMP.text = (int)((float)Health / (float)MaxHealth * 100f) + "%";
    }

    public override void OnObjectDestroy()
    {
        _audioManager.PlaySFX("lose");
        _stationsManager.SwitchState<GameOverState>();

        _gameSession.playerProfile.kills = Kills;
        _gameSession.playerProfile.score = Score;
        _gameSession.Save();

        _gameOverMenuController.Open();
    }

    public void ChangeExperience(int value)
    {
        Experience += value;
        Score += value;

        if (Experience >= ExperienceToNextLevel)
        {
            Experience = 0;
            Level++;

            float levelCost = modifiersController.GetModifierValue("base_xp_to_next_level");
            levelCost *= 1f + modifiersController.GetModifierValue("xp_to_next_level_multipler_modifier");

            for (int i = 0; i < Level - 1; i++)
            {
                levelCost *= (float)modifiersController.GetModifierValue("level_xp_cost_multipler"); ;
            }

            ExperienceToNextLevel = (int)levelCost;

            _audioManager.PlaySFX("levelUp");

            upgradesController.OpenUpgrades(Level);
            OnLevelUp?.Invoke();
        }

        levelTMP.text = "LVL " + Level.ToString();
        xpTMP.text = Experience.ToString() + "/" + ExperienceToNextLevel.ToString() + " EXP";
        xpProgressBar.value = (float)Experience / (float)ExperienceToNextLevel;
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

    public void SetMaxHealth(int value)
    {
        MaxHealth = value;

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

    public void SetMaxStamina(int value)
    {
        MaxStamina = value;
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
        if(value > 0 )
        {
            float valueWithMultiplier = (float)value * (1 + modifiersController.GetModifierValue("money_gain_multiplier_modifier"));
            value = (int)valueWithMultiplier;
        }

        Money += value;
        Score += value;

        cashTMP.text = Money.ToString();
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
                OrbController orb = hit.GetComponent<OrbController>();

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
    }
}