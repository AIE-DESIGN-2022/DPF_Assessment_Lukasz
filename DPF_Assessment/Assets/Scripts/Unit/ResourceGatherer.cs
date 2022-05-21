using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGatherer : MonoBehaviour
{
    private CollectableResource _targetResource;
    private Unit _unit;
    private int _gatheredAmmount = 0;
    private CollectableResource.EResourceType _gatheringType;
    private int _gatherRate;


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
        if (_targetResource == null) return;

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
        if (!_targetResource.HasResource()) ClearTargetResource();
        _unit.StopMoveTo();
        if (_gatheringTool == null) EquipTool();
        _unit.Animator().SetBool("working", true);
    }

    public void SetTargetResource(CollectableResource _newResource)
    {
        _targetResource = _newResource;
    }

    public void ClearTargetResource()
    {
        if (_targetResource != null) _targetResource = null;
        if (_unit != null) _unit.Animator().SetBool("working", false);
        UnequipTool();
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
        if (_targetResource.ResourceType() != _gatheringType)
        {
            _gatheringType = _targetResource.ResourceType();
            _gatheredAmmount = 0;
        }
        
        _gatheredAmmount += _targetResource.Gather(_gatherRate);
    }
}
