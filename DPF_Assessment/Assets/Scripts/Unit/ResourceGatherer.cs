using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGatherer : MonoBehaviour
{
    private CollectableResource _targetResource;
    private CollectableResource _lastTargetResource;
    private Unit _unit;
    private int _gatheredAmount = 0;
    private CollectableResource.EResourceType _gatheringType;
    private int _gatherRate;
    private Building _dropOff;


    [SerializeField] private float _gatherRange = 2.0f;
    [SerializeField] private int _maxCarry = 10;
    [SerializeField] private EquipmentConfig _woodCuttingTool;
    [SerializeField] private int _woodGatherRate = 1;

    private GameObject _gatheringTool;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    private void Update()
    {
        Gathering();
        DropOff();
    }

    private void DropOff()
    {
        if (_targetResource == null && _dropOff != null)
        {
            if (_gatheredAmount > 0)
            {
                if (_unit != null && _unit.Animator().GetBool("working")) _unit.Animator().SetBool("working", false);
                _unit.MoveTo(_dropOff);
            }
        }
    }

    private void Gathering()
    {
        if (_targetResource == null)
        {
            return;
        }    

        if (IsInRange())
        {
            GatherResource();
        }
        else
        {
            _unit.MoveTo(_targetResource);
        }
    }

    private void GatherResource()
    {
        if (_gatheredAmount >= _maxCarry)
        {
            ClearTargetResource(true);
            return;
        }
        if (!_targetResource.HasResource())
        {
            ClearTargetResource();
            return;
        }
        _unit.StopMoveTo();
        if (_gatheringTool == null) EquipTool();
        _unit.Animator().SetBool("working", true);
    }

    public void SetTargetResource(CollectableResource _newResource)
    {
        _targetResource = _newResource;
        SetTargetDropOffPoint(FindObjectOfType<GameController>().GetFaction(_unit.PlayerNumber()).ClosestResourceDropPoint(_targetResource));
    }

    public void SetTargetDropOffPoint(Building building)
    {
        _dropOff = building;
    }    

    public void ClearTargetResource(bool _rememberForLater = false)
    {
        if (_targetResource != null)
        {
            if (_rememberForLater) _lastTargetResource = _targetResource;
            _targetResource = null;
        }
        if (_unit != null) _unit.Animator().SetBool("working", false);
        else Debug.LogError(gameObject.name + " Gatherer's unit referance missing.");
        UnequipTool();
    }

    public void ClearDropOffPoint()
    {
        if (_dropOff != null) _dropOff = null;
    }

    private bool IsInRange()
    {
        if (_targetResource != null)
        {
            float _distance = Vector3.Distance(transform.position, _targetResource.transform.position);
            return _distance < _gatherRange;
        }
        else return false;
    }

    private void EquipTool()
    {
        if (_targetResource != null)
        {
            CollectableResource.EResourceType _type = _targetResource.ResourceType();

            switch(_type)
            {
                case CollectableResource.EResourceType.Wood:
                    if (_woodCuttingTool != null) _gatheringTool = _woodCuttingTool.Spawn(_unit);
                    _gatherRate = _woodGatherRate;
                    break;

                case CollectableResource.EResourceType.Food:

                    break;

                case CollectableResource.EResourceType.Gold:

                    break;
            }
        }
    }

    private void UnequipTool()
    {
        if (_gatheringTool != null)
        {
            Destroy(_gatheringTool);
            _gatheringTool = null;
        }
    }

    private void GatherEffect()
    {
        if (_gatheredAmount >= _maxCarry) return;

        if (_targetResource.ResourceType() != _gatheringType)
        {
            _gatheringType = _targetResource.ResourceType();
            _gatheredAmount = 0;
        }
        
        if (_gatheredAmount + _gatherRate > _maxCarry)
        {
            _gatheredAmount += _targetResource.Gather(_maxCarry - _gatheredAmount);
        }
        else
        {
            _gatheredAmount += _targetResource.Gather(_gatherRate);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<Building>() == _dropOff)
        {
            _gatheredAmount = 0;
            _unit.StopMoveTo();

            if (_lastTargetResource != null && _lastTargetResource.HasResource())
            {
                SetTargetResource(_lastTargetResource);
                _lastTargetResource = null;
            }
        }
    }

    public int GatheredResourcesAmount()
    {
        return _gatheredAmount;
    }

    public CollectableResource.EResourceType GatheredResourceType()
    {
        return _gatheringType;
    }
}
