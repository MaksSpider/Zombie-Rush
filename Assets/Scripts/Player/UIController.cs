using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    private PlayerController playerController;
    private PlayerCombatController playerCombatController;

    [Header("Transforms")]
    [SerializeField] public RectTransform overlay;
    [SerializeField] public RectTransform expIndicator;
    [SerializeField] public RectTransform killsIndicator;

    [Header("Debug / WIP")]
    [SerializeField] private TextMeshProUGUI cashTMP;
    [SerializeField] private TextMeshProUGUI killsTMP;
    [SerializeField] private TextMeshProUGUI levelTMP;
    [SerializeField] private TextMeshProUGUI hpTMP;
    [SerializeField] private TextMeshProUGUI ammoTMP;
    [SerializeField] private TextMeshProUGUI staminaTMP;

    [Header("Upgrades")]
    [SerializeField] private Image upgradeCard0;
    [SerializeField] private Image upgradeCard1;
    [SerializeField] private Image upgradeCard2;

    [SerializeField] private Sprite commonCardSprite;
    [SerializeField] private Sprite rareCardSprite;
    [SerializeField] private Sprite epicCardSprite;
    [SerializeField] private Sprite legendartyCardSprite;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerController = gameObject.GetComponent<PlayerController>();
        playerCombatController = gameObject.GetComponent<PlayerCombatController>();
    }

    void Update()
    {
        UpdateStatsUI();
    }

    private void UpdateStatsUI()
    {
        var result = playerCombatController.GetCurrentAmmo();

        killsTMP.text = playerController.Kills + "";
        hpTMP.text = (float)playerController.Health / (float)playerController.MaxHealth * 100 + "%";
        staminaTMP.text = (float)playerController.Stamina / (float)playerController.MaxStamina * 100 + "%";

        cashTMP.text = "Cash: " + playerController.Money;
        levelTMP.text = "Level: " + playerController.Level + " " + playerController.Experience + "/" + playerController.ExperienceToNextLevel;
        
        if(!result.found)
        {
            ammoTMP.text = "";
            return;
        }

        if (playerCombatController.currentWeapon is Weapon weapon)
        {
            ammoTMP.text = result.inMagazine + "/" + weapon.magazineSize + " <" + result.all + ">";
        }
        else if (playerCombatController.currentWeapon is MeleeWeapon)
        {
            ammoTMP.text = "";
        }
    }
}
