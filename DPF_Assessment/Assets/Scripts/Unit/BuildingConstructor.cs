// Writen by Lukasz Dziedziczak
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingConstructor : MonoBehaviour
{
    [SerializeField] private EquipmentConfig _constructionTool;
    [SerializeField] private float _buildRate = 100;

    private Unit _unit;
    private Building _currentBuildTarget;
    private bool _buildingIsInRange = false;
    private GameObject _tool;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    private void Update()
    {
        CheckBuildState();
        ProcessBuilding();
    }

    private void CheckBuildState()
    {
        if (_currentBuildTarget == null)
        {
            if (_unit != null && _unit.Animator() != null && _unit.Animator().GetBool("building"))
            {
                _unit.Animator().SetBool("building", false);
                _unit.HUD_StatusUpdate();
            }
            return;
        }

        if (_currentBuildTarget.BuildState() != Building.EBuildState.Building)
        {
            ClearBuildTarget();
            _unit.TakeAStepBack();
        }
    }

    private void ProcessBuilding()
    {
        if (_currentBuildTarget == null) return;

        if (_buildingIsInRange)
        {
            ConstructBuilding();
        }
        else
        {
            _unit.MoveTo(_currentBuildTarget);
            if (_unit != null && _unit.Animator() != null && _unit.Animator().GetBool("building"))
            {
                _unit.Animator().SetBool("building", false);
            }
        }

    }

    private void ConstructBuilding()
    {
        if (_unit != null && _unit.Animator() != null & !_unit.Animator().GetBool("building"))
        {
            _unit.Animator().SetBool("building", true);
            transform.forward = _currentBuildTarget.transform.position;
            _unit.HUD_BuildingStatusUpdate();
        }
        EquipTool();
    }

    public void ClearBuildTarget()
    {
        _currentBuildTarget = null;
        _buildingIsInRange = false;
        UnequipTool();
        _unit.HUD_BuildingStatusUpdate();
    }

    public void SetBuildTarget(Building newTarget)
    {
        _currentBuildTarget = newTarget;
        _unit.HUD_BuildingStatusUpdate();
    }

    public void BuildEffect()
    {
        if (_currentBuildTarget != null)
        {
            _currentBuildTarget.ConstructBuilding(_buildRate);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Building _building = other.gameObject.GetComponent<Building>();
        if (_building != null && _building == _currentBuildTarget)
        {
            _buildingIsInRange = true;
            _unit.StopMoveTo();
        }
    }

    public bool HasBuildTarget() { return _currentBuildTarget != null; }

    public Building BuildTarget() { return _currentBuildTarget; }

    public bool IsConstructingBuilding() { return _unit.Animator().GetBool("building"); }

    private void EquipTool()
    {
        if (_currentBuildTarget != null && _constructionTool != null && _tool == null)
        {
            _tool = _constructionTool.Spawn(_unit);
        }
    }

    private void UnequipTool()
    {
        if (_tool != null)
        {
            Destroy(_tool.gameObject);
            _tool = null;
        }
    }
}
// Writen by Lukasz Dziedziczak