using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableResource : Selectable
{
    [SerializeField] private EResourceType _resourceType;
    [SerializeField] private int _amount;

    private int _currentAmount;

    private new void Start()
    {
        base.Start();
        _currentAmount = _amount;
    }

    public enum EResourceType
    {
        Wood,
        Food,
        Gold
    }

    public EResourceType ResourceType() { return _resourceType; }

    public int Gather(int _gatheringAmount)
    {
        int _returnAmount = 0;
        if (_currentAmount - _gatheringAmount > 0)
        {
            _returnAmount = _gatheringAmount;
            _currentAmount -= _gatheringAmount;
        }
        else
        {
            _returnAmount = _currentAmount;
            _currentAmount = 0;
            ResourceDepleted();
        }
        return _returnAmount;
    }

    private void ResourceDepleted()
    {
        Debug.LogWarning("Resource Depleted");
        throw new NotImplementedException();
    }

    public bool HasResource() { return _currentAmount > 0; }
}
