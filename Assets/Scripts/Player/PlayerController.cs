using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

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

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private InputManager inputManager;
    private StatusesController statusesController;

    public event System.Action OnLevelUp;

    [SerializeField] Vector3 SpawnPosition;
    [SerializeField] float baseSpeed = 5f;
    [SerializeField] float sprintBoost = 1.5f;

    [SerializeField] private float collectExpRadius = 1.5f;

    [SerializeField] private int baseLevelCost = 50;
    [SerializeField] private float levelExpCostMultipler = 1.25f;
    [SerializeField] private int startingHealth = 100;
    [SerializeField] private int startingStamina = 100;
    [SerializeField] private int healthRegeneration = 2;
    [SerializeField] private int baseStaminaRegeneration = 2;

    [NonSerialized] public Vector3 MovementDirection;
    [NonSerialized] public Vector2 LastMovementDirection = Vector2.down;

    private Animator animator; 

    public int Health;
    public int MaxHealth;
    public int Money;
    public int Kills;
    public int Stamina;
    public int MaxStamina;
    public int Experience;
    public int ExperienceToNextLevel;
    public int Level = 1;

    private Rigidbody2D rb;

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

        inputManager = InputManager.Instance;
        statusesController = StatusesController.Instance;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        rb.position = SpawnPosition;

        ExperienceToNextLevel = baseLevelCost;
        ChangeMaxStamina(startingStamina);
        ChangeMaxHealth(startingHealth);

        InvokeRepeating(nameof(On5seconds), 0f, 5f);
        InvokeRepeating(nameof(On1seconds), 0f, 1f);
    }

    void Update()
    {
        HandleMovementControlls();
        CollectExperience();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void On1seconds()
    {
        CheckTemporaryHealing();
        ChangeStamina(baseStaminaRegeneration);
    }

    private void On5seconds()
    {
        ChangeHealth(healthRegeneration);
    }

    public void AddExperience(int value)
    {
        Experience += value;

        if(Experience >= ExperienceToNextLevel)
        {
            Experience = 0;
            Level++;

            float levelCost = baseLevelCost;

            for (int i = 0; i < Level; i++) 
            {
                levelCost *= levelExpCostMultipler;
            }

            ExperienceToNextLevel = (int)levelCost;

            OnLevelUp?.Invoke();
        }
    }

    // TODO: implement death mechanic
    public void ChangeHealth(int value)
    {
        Health += value;

        if(Health > MaxHealth) 
        {
            Health = MaxHealth; 
        }

        if (Health <= 0)
        {
            print("dead!");
            // TBA
        }
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
        healthRegeneration += value;
        if(healthRegeneration < 0) { healthRegeneration = 0; }
    }

    public void AddTemporaryHealing(float duration, int additionalMaxHp, int additionalRegeneration, Sprite icon = null)
    {
        temporaryHealing.Add(new TemporaryHealing(Time.time + duration, additionalMaxHp, additionalRegeneration));

        ChangeMaxHealth(additionalMaxHp);
        ChangeRegeneration(additionalRegeneration);

        if (icon == null) return;

        statusesController.RegisterStatus(icon, duration, new Color(255, 0 , 0));
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

        if(Stamina <= 0)
        {
            Stamina = 0;
            staminaRegeneration = true;
        }

        if(Stamina >= MaxStamina * 0.3)
        {
            staminaRegeneration = false;
        }
    }

    public void ChangeMoney(int value)
    {
        Money += value;
    }

    private void HandleMovementControlls()
    {
        Vector2 input = inputManager.InputActions.Player.Move.ReadValue<Vector2>();
        MovementDirection = new Vector3(input.x, input.y, 0f).normalized;

        if (MovementDirection.sqrMagnitude > 0f)
        {
            LastMovementDirection = MovementDirection;
        }

        animator.SetFloat("DirectionHorizontal", LastMovementDirection.x);
        animator.SetFloat("DirectionVertical", LastMovementDirection.y);
    }

    private void HandleMovement()
    {
        float speed = baseSpeed;

        if (Stamina > 0 && !staminaRegeneration && inputManager.InputActions.Player.Sprint.IsPressed()) 
        {
            speed *= sprintBoost;
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

        rb.MovePosition(rb.position + (Vector2)MovementDirection * speed * Time.fixedDeltaTime);
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collectExpRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("ExpOrb"))
            {
                ExpOrbController orb = hit.GetComponent<ExpOrbController>();
                if (orb != null)
                {
                    orb.Collect();
                }
            }
        }
    }
}