using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Building : Selectable
{
    [SerializeField] private EBuildingType _buildingType;
    [SerializeField] private bool _resourceDropPoint = false;

    private UnitProducer _unitProducer;

    private new void Awake()
    {
        base.Awake();
        _unitProducer = GetComponent<UnitProducer>();
    }

    public enum EBuildingType
    {
        TownCenter,
        Barraks,
        University,
        Farm,
        Tower
    }

    public EBuildingType BuildingType() { return _buildingType; }

    public bool IsResourceDropPoint() { return _resourceDropPoint; }

    public void Status(out string _status1, out string _status2)
    {
        _status1 = "";
        _status2 = "";

        if (_unitProducer != null)
        {
            if (_unitProducer.IsCurrentlyProducing())
                _status1 = "Producing " + _unitProducer.CurrentlyProducing().ToString() + "...";
        }

        if (_status1 == "")
        {
            _status1 = "Idle";
        }
    }
}
