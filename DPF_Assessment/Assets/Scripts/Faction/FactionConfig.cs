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
        public Building buildingPrefab;
    }

    [System.Serializable]
    public class FactionUnit
    {
        public Unit.EUnitType unitType;
        public Building.EBuildingType builtFrom;
        public float buildTime = 10;
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
            int _prefabIndex = Random.Range(0, _possiblePrefabs.Count - 1);

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

    public List<Unit.EUnitType> BuildableUnits(Building.EBuildingType buildingType)
    {
        List<Unit.EUnitType> list = new List<Unit.EUnitType>();

        foreach (FactionUnit _factionUnit in _units)
        {
            if (_factionUnit.builtFrom == buildingType) list.Add(_factionUnit.unitType);
        }

        return list;
    }
}
