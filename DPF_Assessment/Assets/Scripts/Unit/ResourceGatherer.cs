// Writen by Lukasz Dziedziczak
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
    private Faction _faction;
    private bool _isInRange = false;

    /*[SerializeField] private float _gatherRange = 2.0f;*/
    [SerializeField] private int _maxCarry = 10;
    [SerializeField] private EquipmentConfig _woodCuttingTool;
    [SerializeField] private int _woodGatherRate = 1;
    [SerializeField] private EquipmentConfig _farmingTool;
    [SerializeField] private int _foodGatherRate = 1;
    [SerializeField] private EquipmentConfig _miningTool;
    [SerializeField] private int _goldGatherRate = 1;

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

        if (_isInRange)
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


        
        switch(_targetResource.ResourceType())
        {
            case CollectableResource.EResourceType.Wood:
                _unit.Animator().SetBool("working", true);
                break;

            case CollectableResource.EResourceType.Food:
                if (_targetResource.GetComponent<Building>() == null)
                {
                    _unit.Animator().SetBool("gathering", true);
                }
                else
                {
                    _unit.Animator().SetBool("working", true);
                }
                break;

            case CollectableResource.EResourceType.Gold:
                _unit.Animator().SetBool("mining", true);
                break;
        }

        transform.LookAt(_targetResource.transform);
    }

    public void SetTargetResource(CollectableResource _newResource, bool isAlreadyAtResource = false)
    {
        _targetResource = _newResource;
        if (_faction == null) SetFaction();
        SetTargetDropOffPoint(_faction.ClosestResourceDropPoint(_targetResource));
        _unit.HUD_StatusUpdate();
        _isInRange = isAlreadyAtResource;
        //print(name + " set target= " + _targetResource);
    }

    public void SetTargetDropOffPoint(Building building)
    {
        _dropOff = building;
        _unit.HUD_StatusUpdate();
    }    

    public void ClearTargetResource(bool _rememberForLater = false)
    {
        if (_targetResource != null)
        {
            if (_rememberForLater) _lastTargetResource = _targetResource;
            _targetResource = null;
        }
        if (_unit != null)
        {
            _unit.Animator().SetBool("working", false);
            _unit.Animator().SetBool("gathering", false);
            _unit.Animator().SetBool("mining", false);
            _unit.HUD_StatusUpdate();
        }
        else Debug.LogError(gameObject.name + " Gatherer's unit referance missing.");

        UnequipTool();
        
        _isInRange = false;
    }

    public void ClearDropOffPoint()
    {
        if (_dropOff != null) _dropOff = null;
        _unit.HUD_StatusUpdate();
        _isInRange=false;
    }

    private bool IsInRange()
    {
        /*if (_targetResource != null)
        {
            float _distance = Vector3.Distance(transform.position, _targetResource.transform.position);
            return _distance < _gatherRange;
        }
        else return false;*/

        return _isInRange;
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
                    if (_farmingTool != null && _targetResource.GetComponent<Building>() != null) _gatheringTool = _farmingTool.Spawn(_unit);
                    _gatherRate = _foodGatherRate;
                    break;

                case CollectableResource.EResourceType.Gold:
                    if (_miningTool != null) _gatheringTool = _miningTool.Spawn(_unit);
                    _gatherRate = _goldGatherRate;
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
            ClearTargetResource();
        }
        else
        {
            _gatheredAmount += _targetResource.Gather(_gatherRate);
        }

        _unit.HUD_StatusUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_dropOff == null) return;

        if (other.gameObject.GetComponent<Building>() == _dropOff)
        {
            _dropOff = null;

            if (_faction == null) SetFaction();
            _faction.AddToStockpile(_gatheringType, _gatheredAmount);
            _gatheredAmount = 0;
            _unit.StopMoveTo();
            _unit.HUD_StatusUpdate();

            if (_lastTargetResource != null && _lastTargetResource.HasResource())
            {
                SetTargetResource(_lastTargetResource);
                _isInRange = false;
                _lastTargetResource = null;
            }
            else
            {
                _unit.TakeAStepBack();
            }
            
        }

        if (other.gameObject.GetComponent<CollectableResource>() == _targetResource)
        {
            _isInRange = true;
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

    private void SetFaction()
    {
        _faction = FindObjectOfType<GameController>().GetFaction(_unit.PlayerNumber());
    }

    public bool HasResourceTarget() { return _targetResource != null; }

    public bool HasDropOffTarget() { return _dropOff != null; }

}
// Writen by Lukasz Dziedziczak