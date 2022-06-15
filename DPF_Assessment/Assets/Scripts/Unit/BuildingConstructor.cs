// Writen by Lukasz Dziedziczak
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingConstructor : MonoBehaviour
{
    [SerializeField] private EquipmentConfig constructionTool;
    [SerializeField] private float buildRate = 100;

    private Unit unit;
    private Building currentBuildTarget;
    private bool buildingIsInRange = false;
    private GameObject tool;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Update()
    {
        CheckBuildState();
        ProcessBuilding();
    }

    private void CheckBuildState()
    {
        if (currentBuildTarget == null)
        {
            if (unit != null && unit.Animator() != null && unit.Animator().GetBool("building"))
            {
                unit.Animator().SetBool("building", false);
                unit.HUD_StatusUpdate();
            }
            return;
        }

        if (currentBuildTarget.BuildState() != Building.EBuildState.Building)
        {
            CollectableResource possibleFarm = currentBuildTarget.GetComponent<CollectableResource>();
            ClearBuildTarget();

            unit.TakeAStepBack();
            if (possibleFarm != null)
            {
                unit.GetComponent<ResourceGatherer>().SetTargetResource(possibleFarm, true);
            }
        }
    }

    private void ProcessBuilding()
    {
        if (currentBuildTarget == null) return;

        if (buildingIsInRange)
        {
            ConstructBuilding();
        }
        else
        {
            unit.MoveTo(currentBuildTarget);
            if (unit != null && unit.Animator() != null && unit.Animator().GetBool("building"))
            {
                unit.Animator().SetBool("building", false);
            }
        }

    }

    private void ConstructBuilding()
    {
        if (currentBuildTarget.IsFarmRebuild())
        {
            if (unit.Faction.CanAfford(Building.EBuildingType.Farm))
            {
                unit.Faction.SubtractFromStockpileCostOf(Building.EBuildingType.Farm);
                currentBuildTarget.FarmRebuild(false);
            }
            else
            {
                ClearBuildTarget();
            }
        }

        if (unit != null && unit.Animator() != null & !unit.Animator().GetBool("building"))
        {
            unit.Animator().SetBool("building", true);
            
            unit.HUD_BuildingStatusUpdate();
        }
        EquipTool();
        //transform.forward = currentBuildTarget.transform.position;
        transform.LookAt(currentBuildTarget.transform.position);
    }

    public void ClearBuildTarget()
    {
        currentBuildTarget = null;
        buildingIsInRange = false;
        UnequipTool();
        unit.HUD_BuildingStatusUpdate();
        unit.HUD_StatusUpdate();
    }

    public void SetBuildTarget(Building newTarget)
    {
        unit.ClearPreviousActions();
        currentBuildTarget = newTarget;
        unit.HUD_BuildingStatusUpdate();
    }

    public void BuildEffect()
    {
        if (currentBuildTarget != null)
        {
            currentBuildTarget.ConstructBuilding(buildRate);
            unit.HUD_BuildingStatusUpdate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Building building = other.gameObject.GetComponent<Building>();
        if (building != null && building == currentBuildTarget)
        {
            buildingIsInRange = true;
            unit.StopMoveTo();
        }
    }

    public bool HasBuildTarget() { return currentBuildTarget != null; }

    public Building BuildTarget() { return currentBuildTarget; }

    public bool IsConstructingBuilding() { return unit.Animator().GetBool("building"); }

    private void EquipTool()
    {
        if (currentBuildTarget != null && constructionTool != null && tool == null)
        {
            tool = constructionTool.Spawn(unit);
        }
    }

    private void UnequipTool()
    {
        if (tool != null)
        {
            Destroy(tool.gameObject);
            tool = null;
        }
    }

    public string CurrentlyBuildingName()
    {
        if (currentBuildTarget != null)
        {
            return unit.Faction.Config().PrefabName(currentBuildTarget.BuildingType());
        }
        else
        {
            return "";
        }
        
    }
}
// Writen by Lukasz Dziedziczak