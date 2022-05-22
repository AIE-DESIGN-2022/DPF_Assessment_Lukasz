using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProducer : MonoBehaviour
{
    [SerializeField] private Transform _spawnTransform;
    [SerializeField] private Transform _rallyPoint;
    [SerializeField] private List<UnitConfig> _buildableUnits;

    private UnitConfig _currentlyBuilding;
    private List<UnitConfig> _buildQue;
    private float _timeLeft;
    private Building _building;

    private void Awake()
    {
        _buildQue = new List<UnitConfig>();
        _building = GetComponent<Building>();
    }

    private void Update()
    {
        ProcessBuildQue();
        ProcessCurrentBuild();
    }

    private void ProcessCurrentBuild()
    {
        if (_currentlyBuilding == null) return;
        _timeLeft -= Time.deltaTime;

        if (_timeLeft <= 0)
        {
            Unit _newUnit = _currentlyBuilding.Spawn(_building.PlayerNumber(), _spawnTransform);
            _currentlyBuilding = null;
            _newUnit.MoveTo(_rallyPoint.position);
            //BuildQueUpdate(building);
        }

    }

    private void ProcessBuildQue()
    {
        if (_currentlyBuilding == null && _buildQue.Count > 0)
        {
            _currentlyBuilding = _buildQue[0];
            _timeLeft = _buildQue[0].BuildTime();
            _buildQue.RemoveAt(0);
            //BuildQueUpdate(building);
        }
    }
}
