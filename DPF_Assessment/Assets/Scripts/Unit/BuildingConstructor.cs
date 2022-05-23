// Writen by Lukasz Dziedziczak
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingConstructor : MonoBehaviour
{
    [SerializeField] private GameObject _buildTool;
    [SerializeField] private float _buildRate;
    [SerializeField] private float _buildDistance = 1.0f;

    private Unit _unit;
    private Building _currentBuildTarget;

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
            }
            return;
        }

        if (_currentBuildTarget.BuildState() != Building.EBuildState.Building)
        {
            ClearBuildTarget();
        }
    }

    private void ProcessBuilding()
    {
        if (_currentBuildTarget == null) return;

        if (BuildingIsWithinRange())
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
        if (_unit != null && _unit.Animator() != null)
        {
            _unit.Animator().SetBool("building", true);
            transform.forward = _currentBuildTarget.transform.position;
        }
    }

    private bool BuildingIsWithinRange()
    {
        float distance = Vector3.Distance(transform.position, _currentBuildTarget.transform.position);
        return distance <= _buildDistance /*+ _currentBuildTarget.NavMeshObstacleRadius()*/;
    }

    /*public void Build(BuildingConfig buildingConfig, List<Builder> builders)
    {
        Transform playerTransform = FindObjectOfType<GameController>().PlayerTransform(_unit.owningPlayerNumber);
        GameObject newObject = Instantiate(buildingConfig.buildingPrefab, playerTransform);
        Building newBuilding = newObject.GetComponent<Building>();
        newBuilding.SetBuildState(Building.EBuildState.Placing);
        FindObjectOfType<GameController>().Player(_unit.owningPlayerNumber).canSelect = false;
        newBuilding.SetBuilders(builders);
    }*/

    public void ClearBuildTarget()
    {
        _currentBuildTarget = null;
    }

    public void SetBuildTarget(Building newTarget)
    {
        _currentBuildTarget = newTarget;
    }

    public void BuildEffect()
    {
        if (_currentBuildTarget != null)
        {
            _currentBuildTarget.ConstructBuilding(_buildRate);
        }
    }
}
// Writen by Lukasz Dziedziczak