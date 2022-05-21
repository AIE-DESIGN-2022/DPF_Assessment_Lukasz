using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    private SpriteRenderer _selectionIndicator;

    [SerializeField, Range(0,4)] private int _owningPlayerNumber = 0;

    protected void Awake()
    {
        _selectionIndicator = GetComponentInChildren<SpriteRenderer>();
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
}
