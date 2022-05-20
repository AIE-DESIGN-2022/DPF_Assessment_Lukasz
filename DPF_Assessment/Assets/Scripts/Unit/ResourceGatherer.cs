using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGatherer : MonoBehaviour
{
    private CollectableResource _targetResource;
    private Unit _unit;

    [SerializeField] private float _gatherRange = 2.0f;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    private void Update()
    {
        GatherResource();
    }

    private void GatherResource()
    {
        if (_targetResource == null) return;

        if (IsInRange())
        {
            _unit.StopMoveTo();
            _unit.Animator().SetBool("working", true);
        }
        else
        {
            _unit.MoveTo(_targetResource);
        }
    }

    public void SetTargetResource(CollectableResource _newResource)
    {
        _targetResource = _newResource;
    }

    public void ClearTargetResource()
    {
        if (_targetResource != null) _targetResource = null;
        if (_unit != null) _unit.Animator().SetBool("working", false);
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
}
