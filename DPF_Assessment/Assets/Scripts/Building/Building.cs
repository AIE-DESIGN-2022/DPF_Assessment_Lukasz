using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Selectable
{
    [SerializeField] private EBuildingType _buildingType;
    [SerializeField] private bool _resourceDropPoint = false;

    public enum EBuildingType
    {
        TownCenter,
        Barraks,
        University,
        Farm,
        Tower
    }

    public EBuildingType BuildingType() { return _buildingType; }
}
