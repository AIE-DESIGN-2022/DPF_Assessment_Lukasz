// Writen by Lukasz Dziedziczak
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Faction")]
public class FactionConfig : ScriptableObject
{
    [SerializeField] private Faction.EFaction faction;
    [SerializeField] private FactionBuilding[] buildings;
    [SerializeField] private FactionUnit[] units;


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
        return faction;
    }

    public Unit GetUnitPrefab(Unit.EUnitType newUnitType)
    {
        List<Unit> possiblePrefabs = new List<Unit>();

        foreach (FactionUnit factionUnit in units)
        {
            if (factionUnit.unitType == newUnitType)
            {
                possiblePrefabs = factionUnit.unitPrefabVariations;
                break;
            }
        }

        if (possiblePrefabs.Count > 1)
        {
            int prefabIndex = Random.Range(0, possiblePrefabs.Count);
            return possiblePrefabs[prefabIndex];
        }
        else
        {
            return possiblePrefabs[0];
        }
        
    }

    public Building GetBuildingPrefab(Building.EBuildingType newBuildingType)
    {
        foreach (FactionBuilding factionBuilding in buildings)
        {
            if (factionBuilding.buildingType == newBuildingType) return factionBuilding.buildingPrefab;
        }

        return null;
    }

    public float BuildTime(Unit.EUnitType newUnitType)
    {
        foreach (FactionUnit factionUnit in units)
        {
            if (factionUnit.unitType == newUnitType)
            {
                return factionUnit.buildTime;
            }
        }

        return 0;
    }

    public List<Unit.EUnitType> BuildableUnits(Building.EBuildingType buildingType)
    {
        List<Unit.EUnitType> list = new List<Unit.EUnitType>();

        if (units == null || units.Length == 0)
        {
            Debug.LogError(name + " has no list of units.");
            return null;
        }

        foreach (FactionUnit factionUnit in units)
        {
            if (factionUnit.builtFrom == buildingType)
            {
                list.Add(factionUnit.unitType);
            }
        }

        return list;
    }

    public List<Building.EBuildingType> ConstructableBuildings(Unit.EUnitType constructor)
    {
        List<Building.EBuildingType> list = new List<Building.EBuildingType>();

        if (buildings == null || buildings.Length == 0)
        {
            Debug.LogError(name + " has no listt of buildings.");
            return null;
        }

        foreach (FactionBuilding factionBuilding in buildings)
        {
            if (factionBuilding.builtBy == constructor)
            {
                list.Add(factionBuilding.buildingType);
            }
        }

        return list;
    }

    public FactionBuilding BuildingConfig(Building.EBuildingType type)
    {
        foreach (FactionBuilding building in buildings)
        {
            if (building.buildingType == type) return building;
        }

        return null;
    }

    public FactionUnit UnitConfig(Unit.EUnitType type)
    {
        foreach (FactionUnit unit in units)
        {
            if (unit.unitType == type) return unit;
        }

        return null;
    }

    public Texture Icon(Building.EBuildingType type)
    {
        foreach (FactionBuilding building in buildings)
        {
            if (building.buildingType == type) return building.buildingIcon;
        }

        return null;
    }

    public Texture Icon(Unit.EUnitType type)
    {
        foreach (FactionUnit unit in units)
        {
            if (unit.unitType == type) return unit.unitIcon;
        }

        return null;
    }

    public string PrefabName(Unit.EUnitType type)
    {
        List<Unit> possiblePrefabs = new List<Unit>();

        foreach (FactionUnit factionUnit in units)
        {
            if (factionUnit.unitType == type)
            {
                possiblePrefabs = factionUnit.unitPrefabVariations;
                break;
            }
        }

        return possiblePrefabs[0].name;
    }

    public string PrefabName(Building.EBuildingType type)
    {
        foreach (FactionBuilding factionBuilding in buildings)
        {
            if (factionBuilding.buildingType == type) return factionBuilding.buildingPrefab.name;
        }

        return null;
    }
}
// Writen by Lukasz Dziedziczak