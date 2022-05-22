using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProducer : MonoBehaviour
{
    [SerializeField] private Transform _spawnTransform;
    [SerializeField] private Transform _rallyPoint;
    [SerializeField] private List<UnitConfig> _buildableUnits;

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
    }

    private void Start()
    {
        SetFaction();
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
            // update HUD build que;
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
            // update HUD build que;
        }
    }

    private void SetFaction()
    {
        if (_building.PlayerNumber() != 0)
        {
            _faction = FindObjectOfType<GameController>().GetFaction(_building.PlayerNumber());
        }
    }

}
