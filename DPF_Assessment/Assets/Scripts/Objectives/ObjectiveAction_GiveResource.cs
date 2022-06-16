using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAction_GiveResource : ObjectiveAction
{
    [SerializeField] private int food = 0;
    [SerializeField] private int wood = 0;
    [SerializeField] private int gold = 0;
    [SerializeField] List<Unit.EUnitType> costOfUnits = new List<Unit.EUnitType>();
    [SerializeField] List<Building.EBuildingType> costOfBuildings = new List<Building.EBuildingType>();

    public override void DoAction()
    {
        base.DoAction();

        GiveResource();
    }

    private void GiveResource()
    {
        Faction playerFaction = gameController.GetPlayerFaction();

        AddCostOfSelectables();

        playerFaction.AddToStockpile(CollectableResource.EResourceType.Food, food);
        playerFaction.AddToStockpile(CollectableResource.EResourceType.Wood, wood);
        playerFaction.AddToStockpile(CollectableResource.EResourceType.Gold, gold);
    }

    private void AddCostOfSelectables()
    {
        FactionConfig playerFaction = gameController.GetPlayerFaction().Config();

        if (costOfUnits.Count > 0)
        { 
            foreach (Unit.EUnitType unitType in costOfUnits)
            {
                FactionConfig.FactionUnit unitConfig = playerFaction.UnitConfig(unitType);
                
                if (unitConfig != null)
                {
                    food += unitConfig.foodCost;
                    wood += unitConfig.woodCost;
                    gold += unitConfig.goldCost;
                }
            }
        }

        if (costOfBuildings.Count > 0)
        {
            foreach (Building.EBuildingType buildingType in costOfBuildings)
            {
                FactionConfig.FactionBuilding buildingConfig = playerFaction.BuildingConfig(buildingType);

                if (buildingConfig != null)
                {
                    food += buildingConfig.foodCost;
                    wood += buildingConfig.woodCost;
                    gold += buildingConfig.goldCost;
                }
            }
        }
    }
}
