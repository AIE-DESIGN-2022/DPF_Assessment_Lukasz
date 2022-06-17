// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    [SerializeField] private EFaction faction;
    [SerializeField] private int playerNumber;
    [Header("Resource Stockpile")]
    [SerializeField] private int wood;
    [SerializeField] private int food;
    [SerializeField] private int gold;

    private FactionConfig config;
    public List<Unit> units = new List<Unit>();
    public List<Building> buildings = new List<Building>();
    private GameController gameController;
    private bool placingBuilding = false;
    private bool gameStarted = false;
    private StatSystem statSystem;

    public delegate void UnitCreatedDelegate(Unit.EUnitType unitType);
    public static UnitCreatedDelegate newUnitCreated;

    public delegate void BuildingCreatedDelegate(Building.EBuildingType buildingType);
    public static BuildingCreatedDelegate newBuildingCreated;

    public enum EFaction
    {
        Human,
        Goblin
    }

    private void Awake()
    {
        LoadFactionConfig();
        statSystem = FindObjectOfType<StatSystem>();
    }

    // Start is called before the first frame update
    void Start()
    {
        NameFaction();
        SetupChildren();

        if (config == null) Debug.LogError(name + " missing config.");
    }

    private void SetupChildren()
    {
        if (buildings.Count > 0 || units.Count > 0) return;

        Selectable[] children = GetComponentsInChildren<Selectable>();
        foreach (Selectable child in children)
        {
            child.Setup(playerNumber, this);
            
            Unit unit = child.GetComponent<Unit>();
            if (unit != null) units.Add(unit);
            else
            {
                Building building = child.GetComponent<Building>();
                if (building != null) buildings.Add(building);
            }
        }

        if (units.Count > 0 || buildings.Count > 0)
        {
            gameStarted = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfFactionIsAlive();
    }

    private void CheckIfFactionIsAlive()
    {
        if (gameStarted && units.Count == 0 && buildings.Count == 0)
        {
            gameStarted = false;
            gameController.FactionDefated(this);
        }
    }

    public EFaction FactionType()
    {
        return faction;
    }

    public void SetupFaction(EFaction newFaction, int playerNo)
    {
        faction = newFaction;
        playerNumber = playerNo;
        NameFaction();
        LoadFactionConfig();
    }

    private void NameFaction()
    {
        gameObject.name = "Faction(" + playerNumber + ")" + faction.ToString();
    }

    public int PlayerNumber() { return playerNumber; }

    public Unit SpawnUnit(Unit.EUnitType newUnitType, Transform spawnLocation)
    {
        if (newUnitCreated != null) newUnitCreated(newUnitType);
        Unit newUnit = Instantiate(config.GetUnitPrefab(newUnitType), spawnLocation.position, spawnLocation.rotation);
        newUnit.Setup(playerNumber, this);
        newUnit.transform.parent = transform;
        units.Add(newUnit);
        //newUnitCreated(newUnit.UnitType());
        return newUnit;
    }

    public Building SpawnBuilding(Building.EBuildingType newType, List<BuildingConstructor> builders)
    {
        if (!placingBuilding)
        {
            placingBuilding = true;
            Building newBuilding = Instantiate(config.GetBuildingPrefab(newType));
            newBuilding.Setup(playerNumber, this);
            newBuilding.transform.parent = transform;
            buildings.Add(newBuilding);
            newBuilding.SetBuildState(Building.EBuildState.Placing);
            newBuilding.SetConstructionTeam(builders);
            gameController.PlayerController().PlayerControl(false);
            return newBuilding;
        }
        else return null;
    }

    public Building SpawnFirstBuilding(Vector3 spawnLocartion)
    {
        Building newBuilding = Instantiate(config.GetBuildingPrefab(Building.EBuildingType.TownCenter));
        newBuilding.Setup(playerNumber, this);
        newBuilding.transform.parent = transform;
        buildings.Add(newBuilding);
        newBuilding.SetBuildState(Building.EBuildState.Complete);
        newBuilding.transform.position = spawnLocartion;
        gameStarted = true;
        return newBuilding;
    }

    public bool CurrentlyPlacingBuilding()
    {
        return placingBuilding;
    }

    public void FinishBuildingPlacement() 
    {
        gameController.PlayerController().PlayerControl(true);
        placingBuilding = false; 
    }

    public void BuildingConstructionComplete(Building newBuilding)
    {
        if(newBuildingCreated != null) newBuildingCreated(newBuilding.BuildingType());
    }

    public FactionConfig Config()
    {
        if (config != null)
        {
            return config;
        }
        else
        {
            Debug.LogError(gameObject.name + " missing FactionConfig");
            return null;
        }
    }

    public Building ClosestResourceDropPoint(CollectableResource collectableResource)
    {
        List<Building> dropPoints = new List<Building>();

        if (buildings.Count <= 0)
        {
            Debug.LogError(gameObject.name + " has no list of buildings.");
            return null;
        }

        foreach (Building building in buildings)
        {
            if (building.IsResourceDropPoint() && building.ConstructionComplete()) dropPoints.Add(building);
        }

        Building closest = null;
        float smallestDistance = Mathf.Infinity;

        foreach (Building dropPoint in dropPoints)
        {
            float distance = Vector3.Distance(dropPoint.transform.position, collectableResource.transform.position);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closest = dropPoint;
            }
        }

        return closest;
    }

    public void CancelBuildingPlacement(Building building)
    {
        placingBuilding = false;
        buildings.Remove(building);
        AddToStockpileCostOf(building.BuildingType());
        gameController.PlayerController().PlayerControl(true);
        Destroy(building.gameObject);
    }

    public int StockpileAmount(CollectableResource.EResourceType resource_type)
    {
        switch(resource_type)
        {
            case CollectableResource.EResourceType.Food:
                return food;

            case CollectableResource.EResourceType.Wood:
                return wood;

            case CollectableResource.EResourceType.Gold:
                return gold;

            default:
                Debug.LogError(name + " could not find resource of type: " + resource_type);
                return 0;
        }    
    }

    public void AddToStockpile(CollectableResource.EResourceType type, int amount)
    {
        switch (type)
        {
            case CollectableResource.EResourceType.Food:
                food += amount;
                break;

            case CollectableResource.EResourceType.Wood:
                wood += amount;
                break;

            case CollectableResource.EResourceType.Gold:
                gold += amount;
                break;

            default:
                Debug.Log("Unable to add to Faction Stockpile, unknown resource type.");
                break;
        }

        if (gameController.IsPlayerFaction(playerNumber)) gameController.HUD_Manager().UpdateResources();
    }

    public void RemoveFromStockpile(CollectableResource.EResourceType type, int amount)
    {
        switch (type)
        {
            case CollectableResource.EResourceType.Food:
                food -= amount;
                break;

            case CollectableResource.EResourceType.Wood:
                wood -= amount;
                break;

            case CollectableResource.EResourceType.Gold:
                gold -= amount;
                break;

            default:
                Debug.Log("Unable to add to Faction Stockpile, unknown resource type.");
                break;
        }

        if (gameController.IsPlayerFaction(playerNumber)) gameController.HUD_Manager().UpdateResources();
    }


    public void SetGameController(GameController controller)
    {
        gameController =  controller;
    }

    public void SetFactionConfig(FactionConfig newConfig)
    {
        Debug.Log(name + " setting new config as " + newConfig.ToString());
        config = newConfig;
    }    

    public void Death(Selectable deadSelectable)
    {
        Unit unit = deadSelectable.GetComponent<Unit>();
        if (unit != null)
        {
            units.Remove(unit);
            if (FindObjectOfType<GameController>().IsPlayerFaction(playerNumber))
            {

            }
            else
            {
                statSystem.AddStatKilled(unit.UnitType());
            }
        }
        else
        {
            Building building = deadSelectable.GetComponent<Building>();
            if (building != null)
            {
                buildings.Remove(building);
                if (FindObjectOfType<GameController>().IsPlayerFaction(playerNumber))
                {

                }
                else
                {
                    statSystem.AddStatKilled(building.BuildingType());
                }
            }
        }
    }

    public bool CanAfford(Unit.EUnitType newUnit)
    {
        FactionConfig.FactionUnit unitConfig = config.UnitConfig(newUnit);

        return food >= unitConfig.foodCost && wood >= unitConfig.woodCost && gold >= unitConfig.goldCost;
    }

    public bool CanAfford(Building.EBuildingType newBuilding)
    {
        FactionConfig.FactionBuilding buildingConfig = config.BuildingConfig(newBuilding);

        return food >= buildingConfig.foodCost && wood >= buildingConfig.woodCost && gold >= buildingConfig.goldCost;
    }
    
    public void SubtractFromStockpileCostOf(Unit.EUnitType newUnit)
    {
        FactionConfig.FactionUnit unitConfig = config.UnitConfig(newUnit);

        food -= unitConfig.foodCost;
        wood -= unitConfig.woodCost;
        gold -= unitConfig.goldCost;

        if (gameController.IsPlayerFaction(playerNumber)) gameController.HUD_Manager().UpdateResources();
    }

    public void AddToStockpileCostOf(Unit.EUnitType newUnit)
    {
        FactionConfig.FactionUnit unitConfig = config.UnitConfig(newUnit);

        food += unitConfig.foodCost;
        wood += unitConfig.woodCost;
        gold += unitConfig.goldCost;

        if (gameController.IsPlayerFaction(playerNumber)) gameController.HUD_Manager().UpdateResources();
    }

    public void SubtractFromStockpileCostOf(Building.EBuildingType newBuilding)
    {
        FactionConfig.FactionBuilding buildingConfig = config.BuildingConfig(newBuilding);

        food -= buildingConfig.foodCost;
        wood -= buildingConfig.woodCost;
        gold -= buildingConfig.goldCost;

        if (gameController.IsPlayerFaction(playerNumber)) gameController.HUD_Manager().UpdateResources();
    }

    public void AddToStockpileCostOf(Building.EBuildingType newBuilding)
    {
        FactionConfig.FactionBuilding buildingConfig = config.BuildingConfig(newBuilding);

        food += buildingConfig.foodCost;
        wood += buildingConfig.woodCost;
        gold += buildingConfig.goldCost;

        if (gameController.IsPlayerFaction(playerNumber)) gameController.HUD_Manager().UpdateResources();
    }

    private void LoadFactionConfig()
    {
        FactionConfig[] loadedConfigs = Resources.LoadAll<FactionConfig>("Factions/");
        //Debug.Log(loadedConfigs.Length + " FactoryConfigs loaded for " + name);
        foreach (var loadedConfig in loadedConfigs)
        {
            //Debug.Log("Checking loadedConfig " + loadedConfig.ToString());
            if (loadedConfig.Faction() == faction)
            {
                //Debug.Log("Found match: loadedConfig " + loadedConfig.Faction().ToString() + " and " + faction.ToString());
                config = loadedConfig;
            }

        }

        if (config == null) Debug.LogError(name + " could not load FactionConfig.");
        if (config.Faction() != faction) Debug.LogError(name + "has FactionConfig mismatch.");
        
    }

    public List<Selectable> Selectables()
    {
        List<Selectable> list = new List<Selectable>();

        foreach (Unit unit in units)
        {
            list.Add(unit);
        }

        foreach (Building building in buildings)
        {
            if (building.BuildState() == Building.EBuildState.Complete)
            {
                list.Add(building);
            }
        }

        return list;
    }

    public List<Selectable> IdleWorkers()
    {
        List<Selectable> list = new List<Selectable>();

        foreach (Unit unit in units)
        {
            if (unit.UnitType() == Unit.EUnitType.Worker && unit.IsIdle())
            {
                list.Add(unit);
            }
        }

        return list;
    }

    public void RemoveFromFaction(Unit oldUnit)
    {
        units.Remove(oldUnit);
    }

    public void RemoveFromFaction(Building oldBuilding)
    {
        buildings.Remove(oldBuilding);
    }

    public void TransferOwnership(Selectable selectable)
    {
        Building building = selectable.GetComponent<Building>();
        Unit unit = selectable.GetComponent<Unit>();
        Faction oldFaction = selectable.Faction;
        selectable.transform.parent = transform;
        selectable.Setup(playerNumber, this);

        if (unit != null)
        {
            oldFaction.RemoveFromFaction(unit);
            units.Add(unit);
        }
        else if (building != null)
        {
            oldFaction.RemoveFromFaction(building);
            buildings.Add(building);
        }

    }
}
// Writen by Lukasz Dziedziczak