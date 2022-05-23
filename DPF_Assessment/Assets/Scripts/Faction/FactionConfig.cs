// Writen by Lukasz Dziedziczak
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Faction")]
public class FactionConfig : ScriptableObject
{
    [SerializeField] private Faction.EFaction _faction;
    [SerializeField] private FactionBuilding[] _buildings;
    [SerializeField] private FactionUnit[] _units;


    [System.Serializable]
    public class FactionBuilding
    {
        public Building.EBuildingType buildingType;
        public Unit.EUnitType builtBy = Unit.EUnitType.Worker;
        public int foodCost = 0;
        public int woodCost = 0;
        public int goldCost = 0;
        public Texture buildingIcon;
        public Building buildingPrefab;
    }

    [System.Serializable]
    public class FactionUnit
    {
        public Unit.EUnitType unitType;
        public Building.EBuildingType builtFrom;
        public float buildTime = 10;
        public int foodCost = 0;
        public int woodCost = 0;
        public int goldCost = 0;
        public Texture unitIcon;
        public List<Unit> unitPrefabVariations;
    }

    public Faction.EFaction Faction()
    {
        return _faction;
    }

    public Unit GetUnitPrefab(Unit.EUnitType _newUnitType)
    {
        List<Unit> _possiblePrefabs = new List<Unit>();

        foreach (FactionUnit _factionUnit in _units)
        {
            if (_factionUnit.unitType == _newUnitType)
            {
                _possiblePrefabs = _factionUnit.unitPrefabVariations;
                break;
            }
        }

        if (_possiblePrefabs.Count > 1)
        {
            int _prefabIndex = Random.Range(0, _possiblePrefabs.Count);
            return _possiblePrefabs[_prefabIndex];
        }
        else
        {
            return _possiblePrefabs[0];
        }
        
    }

    public Building GetBuildingPrefab(Building.EBuildingType _newBuildingType)
    {
        foreach (FactionBuilding _factionBuilding in _buildings)
        {
            if (_factionBuilding.buildingType == _newBuildingType) return _factionBuilding.buildingPrefab;
        }

        return null;
    }

    public float BuildTime(Unit.EUnitType _newUnitType)
    {
        foreach (FactionUnit _factionUnit in _units)
        {
            if (_factionUnit.unitType == _newUnitType)
            {
                return _factionUnit.buildTime;
            }
        }

        return 0;
    }

    public List<Unit.EUnitType> BuildableUnits(Building.EBuildingType _buildingType)
    {
        List<Unit.EUnitType> _list = new List<Unit.EUnitType>();

        if (_units == null || _units.Length == 0)
        {
            Debug.LogError(name + " has no list of units.");
            return null;
        }

        foreach (FactionUnit _factionUnit in _units)
        {
            if (_factionUnit.builtFrom == _buildingType)
            {
                _list.Add(_factionUnit.unitType);
            }
        }

        return _list;
    }

    public List<Building.EBuildingType> ConstructableBuildings(Unit.EUnitType _constructor)
    {
        List<Building.EBuildingType> _list = new List<Building.EBuildingType>();

        if (_buildings == null || _buildings.Length == 0)
        {
            Debug.LogError(name + " has no listt of buildings.");
            return null;
        }

        foreach (FactionBuilding _factionBuilding in _buildings)
        {
            if (_factionBuilding.builtBy == _constructor)
            {
                _list.Add(_factionBuilding.buildingType);
            }
        }

        return _list;
    }

    public FactionBuilding BuildingConfig(Building.EBuildingType _type)
    {
        foreach (FactionBuilding _building in _buildings)
        {
            if (_building.buildingType == _type) return _building;
        }

        return null;
    }

    public FactionUnit UnitConfig(Unit.EUnitType _type)
    {
        foreach (FactionUnit _unit in _units)
        {
            if (_unit.unitType == _type) return _unit;
        }

        return null;
    }

    public Texture Icon(Building.EBuildingType _type)
    {
        foreach (FactionBuilding _building in _buildings)
        {
            if (_building.buildingType == _type) return _building.buildingIcon;
        }

        return null;
    }

    public Texture Icon(Unit.EUnitType _type)
    {
        foreach (FactionUnit _unit in _units)
        {
            if (_unit.unitType == _type) return _unit.unitIcon;
        }

        return null;
    }
}
// Writen by Lukasz Dziedziczak