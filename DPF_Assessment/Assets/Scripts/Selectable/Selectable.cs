// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)),RequireComponent(typeof(Collider))]
public class Selectable : MonoBehaviour
{
    private SpriteRenderer selectionIndicator;
    private Color selectionColor;
    private MinimapIndicator minimapIndicator;

    [Tooltip("The player number whom owns this unit. 0 = none")]
    [SerializeField, Range(0,4)] private int owningPlayerNumber = 0;
    protected Faction owningFaction;
    protected Health health;
    protected GameController gameController;

    [SerializeField] protected bool flashing = false;
    private float flashingSpeed = 0.1f;
    private float flashingTimer = 0f;
    private float flashFor = 2;
    private float flashForTimer = 0;
    private bool flashingUntillSelected = false;

    [SerializeField] protected float sightDistance = 15;

    private MeshRenderer rallyPointFlag;

    protected void Awake()
    {
        selectionIndicator = GetComponentInChildren<SpriteRenderer>();
        health = GetComponent<Health>();
        gameController = FindObjectOfType<GameController>();
        selectionColor = selectionIndicator.color;
        minimapIndicator = GetComponentInChildren<MinimapIndicator>();
    }

    // Start is called before the first frame update
    protected void Start()
    {
        if (selectionIndicator == null) Debug.Log(name + " is missing Selection Indicator");

        Selected(false);
        RigidbodySetup();
        ColliderSetup();
        
    }

    protected void Update()
    {
        Flashing();
    }

    private void RigidbodySetup()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void ColliderSetup()
    {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    public void Selected(bool isSelected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.enabled = isSelected;
        }

        if (isSelected && flashingUntillSelected)
        {
            flashingUntillSelected = false;
            flashing = false;
        }

        ShowRallyPointFlag(isSelected);
    }

    public bool IsSelected() { return selectionIndicator.enabled; }

    public void Setup(int newPlayerNumber, Faction newFaction)
    {
        owningPlayerNumber = newPlayerNumber;
        owningFaction = newFaction;
        SetMinimapIndicator();
    }

    public void SetMinimapIndicator()
    {
        if (minimapIndicator)
        {
            if (gameController.IsPlayerFaction(PlayerNumber()))
            {
                minimapIndicator.SetColor(gameController.PlayerController().PlayerColor());
            }
            else
            {
                minimapIndicator.SetColor(Color.red);
            }

            minimapIndicator.SetSize();
        }
        else
        {
            Debug.LogError(name + " is missing miniMapIndicator");
        }
    }

    public int PlayerNumber() { return owningPlayerNumber; }

    public Faction Faction { get { return owningFaction; } }

    public void TakeDamage(float damageAmound, Selectable attacker)
    {
        if (health != null)
        {
            health.TakeDamage(damageAmound);
        }
        HUD_HealthBarUpdate();

        Unit unit = gameObject.GetComponent<Unit>();
        if (unit != null && unit.UnitStance() != Unit.EUnitStance.Passive)
        {
            Attacker defender = unit.GetComponent<Attacker>();
            if (defender != null && !defender.HasTarget())
            {
                defender.SetTarget(attacker);
            }
        }

        
    }

    public void Heal(float healAmount)
    {
        if (health != null)
        {
            health.Heal(healAmount);
        }
        HUD_HealthBarUpdate();
    }

    public bool IsAlive() 
    { 
        if (health != null) return health.IsAlive();
        else return false;
    }

    private void DeathEnd()
    {
        FindObjectOfType<GameController>().GetFaction(owningPlayerNumber).Death(this);
        Destroy(gameObject);
    } 
    
    public void HUD_StatusUpdate()
    {
        if (IsSelected())
        {
            FindObjectOfType<GameController>().HUD_Manager().Info_HUD().UpdateStatus();
        }
    }

    public void HUD_BuildingStatusUpdate()
    {
        if (IsSelected())
        {
            FindObjectOfType<GameController>().HUD_Manager().Info_HUD().UpdateBuildingStatus();
        }
        HUD_StatusUpdate();
    }

    public void HUD_BuildingQueUpdate()
    {
        if (IsSelected())
        {
            FindObjectOfType<GameController>().HUD_Manager().Info_HUD().UpdateBuildQue();
        }
    }

    public void HUD_HealthBarUpdate()
    {
        if (IsSelected())
        {
            FindObjectOfType<GameController>().HUD_Manager().Info_HUD().UpdateHealthBar();
            FindObjectOfType<GameController>().HUD_Manager().InfoMulti_HUD().UpdateHealthBar(this);
        }
    }

    private void Flashing()
    {
        if (flashing)
        {
            flashingTimer += Time.deltaTime;
            flashForTimer += Time.deltaTime;

            if (flashingTimer > flashingSpeed)
            {
                if (selectionIndicator != null && selectionIndicator.enabled)
                {
                    selectionIndicator.enabled = false;
                }
                else if (selectionIndicator != null && !selectionIndicator.enabled)
                {
                    selectionIndicator.enabled = true;
                }

                flashingTimer = 0;
            }

            if (flashFor != 0 && flashForTimer > flashFor)
            {
                SetFlashing(false);
                flashForTimer = 0;
                flashFor = 0;
                selectionIndicator.color = selectionColor;
                Selected(false);
            }
        }
    }

    public void SetFlashing(bool isFlashing)
    {
        flashing = isFlashing;
    }

    public void StartFlashingFor(float seconds)
    {
        flashFor = seconds;
        SetFlashing(true);
    }

    public void FlashUntillSelected()
    {
        flashFor = 0;
        flashingUntillSelected = true;
        SetFlashing(true);
    }

    public List<Selectable> GetEnemiesInSight()
    {
        List<Selectable> list = new List<Selectable>();
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, sightDistance, Vector3.up);
        foreach (RaycastHit hit in hits)
        {
            Unit unit = hit.transform.GetComponent<Unit>();
            Building building = hit.transform.GetComponent<Building>();

            if (unit != null && unit.PlayerNumber() != PlayerNumber()) list.Add(unit);
            if (building != null && building.PlayerNumber() != PlayerNumber()) list.Add(building);
        }

        return list;
    }

    public void ConfirmOrder()
    {
        StartFlashingFor(1);
    }

    public void ConfirmAttack()
    {
        selectionIndicator.color = Color.red;
        ConfirmOrder();
    }

    public float SightDistance() { return sightDistance; }

    public void Visable(bool isVisable)
    {
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.enabled = isVisable;
        }
    }

    public GameController GameController { get { return gameController; } }

    public void SetRallyPointFlag(MeshRenderer meshRender)
    {
        rallyPointFlag = meshRender;
    }

    public void ShowRallyPointFlag(bool isShowing)
    {
        if (rallyPointFlag != null)
        {
            rallyPointFlag.gameObject.SetActive(isShowing);
        }
    }
}
// Writen by Lukasz Dziedziczak