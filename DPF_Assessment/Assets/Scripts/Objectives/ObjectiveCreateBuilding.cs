using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveCreateBuilding : Objective
{
    [Header("Create Building")]
    [SerializeField] private Building.EBuildingType buildingType;
    [SerializeField] private int amount = 1;

    private int buildingsCreated = 0;

    private new void Start()
    {
        base.Start();
        Faction.newBuildingCreated += NewBuildingCreated;
    }

    private void NewBuildingCreated(Building.EBuildingType newBuildingType)
    {
        if (!isActivated) return;

        if (newBuildingType == buildingType)
        {
            buildingsCreated++;

            if (buildingsCreated == amount)
            {
                ObjectiveComplete();
            }    
        }
    }
}
