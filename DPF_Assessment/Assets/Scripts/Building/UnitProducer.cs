// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProducer : MonoBehaviour
{
    private Transform spawnTransform;
    private Transform rallyPoint;
    
    private List<Unit.EUnitType> buildableUnits;
    private bool isCurrentlyBuilding;
    private Unit.EUnitType currentlyBuilding;
    private List<Unit.EUnitType> buildQue;
    private float timeLeft;
    private Building building;
    private Faction faction;

    private void Awake()
    {
        buildQue = new List<Unit.EUnitType>();
        building = GetComponent<Building>();
        SetTransforms();
    }

    private void SetTransforms()
    {
        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach (Transform transform in transforms)
        {
            if (transform.name == "SpawnPoint") spawnTransform = transform;
            if ( transform.name == "RallyPoint") rallyPoint = transform;
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        ProcessBuildQue();
        ProcessCurrentBuild();
    }

    private void ProcessCurrentBuild()
    {
        if (!isCurrentlyBuilding) return;
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0)
        {
            isCurrentlyBuilding = false;
            Unit newUnit = faction.SpawnUnit(currentlyBuilding, spawnTransform);
            newUnit.MoveTo(rallyPoint.position);
            building.HUD_BuildingStatusUpdate();
        }

    }

    private void ProcessBuildQue()
    {
        if (!isCurrentlyBuilding && buildQue.Count > 0)
        {
            currentlyBuilding = buildQue[0];
            buildQue.RemoveAt(0);
            timeLeft = faction.Config().BuildTime(currentlyBuilding);
            isCurrentlyBuilding = true;
            building.HUD_BuildingStatusUpdate();
            building.HUD_BuildingQueUpdate();
        }
    }

    private void SetFaction()
    {
        if (building.PlayerNumber() != 0 && faction == null)
        {
            faction = FindObjectOfType<GameController>().GetFaction(building.PlayerNumber());
        }
    }

    private void SetListOfBuildableUnits()
    {
        if (faction == null) SetFaction();

        if (faction != null && buildableUnits == null)
        {
            buildableUnits = faction.Config().BuildableUnits(building.BuildingType());
        }
    }

    public List<Unit.EUnitType> GetListOfBuildableUnits()
    {
        if (buildableUnits == null) SetListOfBuildableUnits();

        if (buildableUnits != null)
        {
            return buildableUnits;
        }
        else return null;
    }

    public void AddToQue(Unit.EUnitType newUnit)
    {
        if (buildQue.Count < 20 && CanAfford(newUnit))
        {
            buildQue.Add(newUnit);
            faction.SubtractFromStockpileCostOf(newUnit);
            building.HUD_BuildingQueUpdate();
        }
    }

    public void RemoveFromQue(Unit.EUnitType newUnit)
    {
        if (buildQue.Contains(newUnit))
        {
            buildQue.Remove(newUnit);
            faction.AddToStockpileCostOf(newUnit);
            building.HUD_BuildingQueUpdate();
        }
    }

    public void CancelBuildItem()
    {
        isCurrentlyBuilding = false;
        faction.AddToStockpileCostOf(currentlyBuilding);
        building.HUD_BuildingStatusUpdate();
    }

    public List<Unit.EUnitType> BuildQue()
    {
        return buildQue;
    }

    private bool CanAfford(Unit.EUnitType newUnit)
    {
        if (faction == null) SetFaction();

        return faction.CanAfford(newUnit);
    }

    public bool IsCurrentlyProducing()
    {
        return isCurrentlyBuilding;
    }

    public Unit.EUnitType CurrentlyProducing()
    {
        return currentlyBuilding;
    }

    public float PercentageComplete()
    {
        return 1 - (timeLeft / faction.Config().BuildTime(currentlyBuilding));
    }

    public string CurrentlyBuildingName()
    {
        string name = faction.Config().PrefabName(currentlyBuilding);
        name = name.Replace("(Female)", "");
        name = name.Replace("(Male)", "");
        return name;
    }
}
// Writen by Lukasz Dziedziczak