// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProducer : MonoBehaviour
{
    private Transform _spawnTransform;
    private Transform _rallyPoint;
    
    private List<Unit.EUnitType> _buildableUnits;
    private bool _isCurrentlyBuilding;
    private Unit.EUnitType _currentlyBuilding;
    private List<Unit.EUnitType> _buildQue;
    private float _timeLeft;
    private Building _building;
    private Faction _faction;

    private void Awake()
    {
        _buildQue = new List<Unit.EUnitType>();
        _building = GetComponent<Building>();
        SetTransforms();
    }

    private void SetTransforms()
    {
        Transform[] _transforms = GetComponentsInChildren<Transform>();
        foreach (Transform _transform in _transforms)
        {
            if (_transform.name == "SpawnPoint") _spawnTransform = _transform;
            if ( _transform.name == "RallyPoint") _rallyPoint = _transform;
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        ProcessBuildQue();
        ProcessCurrentBuild();
    }

    private void ProcessCurrentBuild()
    {
        if (!_isCurrentlyBuilding) return;
        _timeLeft -= Time.deltaTime;

        if (_timeLeft <= 0)
        {
            _isCurrentlyBuilding = false;
            Unit _newUnit = _faction.SpawnUnit(_currentlyBuilding, _spawnTransform);
            _newUnit.MoveTo(_rallyPoint.position);
            _building.HUD_BuildingStatusUpdate();
        }

    }

    private void ProcessBuildQue()
    {
        if (!_isCurrentlyBuilding && _buildQue.Count > 0)
        {
            _currentlyBuilding = _buildQue[0];
            _buildQue.RemoveAt(0);
            _timeLeft = _faction.Config().BuildTime(_currentlyBuilding);
            _isCurrentlyBuilding = true;
            _building.HUD_BuildingStatusUpdate();
            _building.HUD_BuildingQueUpdate();
        }
    }

    private void SetFaction()
    {
        if (_building.PlayerNumber() != 0 && _faction == null)
        {
            _faction = FindObjectOfType<GameController>().GetFaction(_building.PlayerNumber());
        }
    }

    private void SetListOfBuildableUnits()
    {
        if (_faction == null) SetFaction();

        if (_faction != null && _buildableUnits == null)
        {
            _buildableUnits = _faction.Config().BuildableUnits(_building.BuildingType());
        }
    }

    public List<Unit.EUnitType> GetListOfBuildableUnits()
    {
        if (_buildableUnits == null) SetListOfBuildableUnits();

        if (_buildableUnits != null)
        {
            return _buildableUnits;
        }
        else return null;
    }

    public void AddToQue(Unit.EUnitType _newUnit)
    {
        if (_buildQue.Count < 20 && CanAfford(_newUnit))
        {
            _buildQue.Add(_newUnit);
            _faction.SubtractFromStockpileCostOf(_newUnit);
            _building.HUD_BuildingQueUpdate();
        }
    }

    public void RemoveFromQue(Unit.EUnitType _newUnit)
    {
        if (_buildQue.Contains(_newUnit))
        {
            _buildQue.Remove(_newUnit);
            _faction.AddToStockpileCostOf(_newUnit);
            _building.HUD_BuildingQueUpdate();
        }
    }

    public void CancelBuildItem()
    {
        _isCurrentlyBuilding = false;
        _faction.AddToStockpileCostOf(_currentlyBuilding);
        _building.HUD_BuildingStatusUpdate();
    }

    public List<Unit.EUnitType> BuildQue()
    {
        return _buildQue;
    }

    private bool CanAfford(Unit.EUnitType _newUnit)
    {
        if (_faction == null) SetFaction();

        return _faction.CanAfford(_newUnit);
    }

    public bool IsCurrentlyProducing()
    {
        return _isCurrentlyBuilding;
    }

    public Unit.EUnitType CurrentlyProducing()
    {
        return _currentlyBuilding;
    }

    public float PercentageComplete()
    {
        return 1 - (_timeLeft / _faction.Config().BuildTime(_currentlyBuilding));
    }

    public string CurrentlyBuildingName()
    {
        string name = _faction.Config().PrefabName(_currentlyBuilding);
        name = name.Replace("(Female)", "");
        name = name.Replace("(Male)", "");
        return name;
    }
}
// Writen by Lukasz Dziedziczak