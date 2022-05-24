// Writen by Lukasz Dziedziczak
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)),RequireComponent(typeof(Collider))]
public class Selectable : MonoBehaviour
{
    private SpriteRenderer _selectionIndicator;

    [Tooltip("The player number whom owns this unit. 0 = none")]
    [SerializeField, Range(0,4)] private int _owningPlayerNumber = 0;
    protected Faction _owningFaction;
    protected Health _health;

    protected void Awake()
    {
        _selectionIndicator = GetComponentInChildren<SpriteRenderer>();
        _health = GetComponent<Health>();
    }

    // Start is called before the first frame update
    protected void Start()
    {
        Selected(false);
        RigidbodySetup();
        ColliderSetup();
        if (_selectionIndicator == null) Debug.Log("Selection Indicator Missing");
    }

    private void RigidbodySetup()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Extrapolate;
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
        if (_selectionIndicator != null)
        {
            _selectionIndicator.enabled = isSelected;
        }
        
    }

    public bool IsSelected() { return _selectionIndicator.enabled; }

    public void Setup(int _newPlayerNumber, Faction _newFaction)
    {
        _owningPlayerNumber = _newPlayerNumber;
        _owningFaction = _newFaction;
    }

    public int PlayerNumber() { return _owningPlayerNumber; }

    public Faction Faction() { return _owningFaction; }

    public void TakeDamage(float _damageAmound)
    {
        if (_health != null)
        {
            _health.TakeDamage(_damageAmound);
        }
        HUD_HealthBarUpdate();
    }

    public void Heal(float _healAmount)
    {
        if (_health != null)
        {
            _health.Heal(_healAmount);
        }
        HUD_HealthBarUpdate();
    }

    public bool IsAlive() 
    { 
        if (_health != null) return _health.IsAlive();
        else return false;
    }

    private void DeathEnd()
    {
        FindObjectOfType<GameController>().GetFaction(_owningPlayerNumber).Death(this);
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
        }
    }


}
// Writen by Lukasz Dziedziczak