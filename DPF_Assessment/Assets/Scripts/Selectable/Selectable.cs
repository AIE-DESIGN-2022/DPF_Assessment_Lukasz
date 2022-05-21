using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    private SpriteRenderer _selectionIndicator;

    [Tooltip("The player number whom owns this unit. 0 = none")]
    [SerializeField, Range(0,4)] private int _owningPlayerNumber = 0;
    Health _health;

    protected void Awake()
    {
        _selectionIndicator = GetComponentInChildren<SpriteRenderer>();
        _health = GetComponent<Health>();
    }

    // Start is called before the first frame update
    protected void Start()
    {
        Selected(false);

        if (_selectionIndicator == null) Debug.Log("Selection Indicator Missing");
    }

    public void Selected(bool isSelected)
    {
        if (_selectionIndicator != null)
        {
            _selectionIndicator.enabled = isSelected;
        }
        
    }

    public bool IsSelected() { return _selectionIndicator.enabled; }

    public void SetPlayerNumber(int _playerNumber)
    {
        _owningPlayerNumber = _playerNumber;
    }

    public int PlayerNumber() { return _owningPlayerNumber; }

    public void TakeDamage(float _damageAmound)
    {
        if (_health != null)
        {
            _health.TakeDamage(_damageAmound);
        }
    }

    public void Heal(float _healAmount)
    {
        if (_health != null)
        {
            _health.Heal(_healAmount);
        }
    }

    public bool IsAlive() 
    { 
        if (_health != null) return _health.IsAlive();
        else return false;
    }

    private void DeathEnd()
    {
        Destroy(gameObject);
    }    
}
