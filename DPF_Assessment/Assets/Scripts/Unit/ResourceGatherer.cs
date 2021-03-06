// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGatherer : MonoBehaviour
{
    public CollectableResource targetResource;
    public CollectableResource lastTargetResource;
    private Unit unit;
    private int gatheredAmount = 0;
    private CollectableResource.EResourceType gatheringType;
    private int gatherRate;
    private Building dropOff;
    private Faction faction;
    private bool isInRange = false;

    /*[SerializeField] private float gatherRange = 2.0f;*/
    [SerializeField] private int maxCarry = 10;
    [SerializeField] private EquipmentConfig woodCuttingTool;
    [SerializeField] private int woodGatherRate = 1;
    [SerializeField] private EquipmentConfig farmingTool;
    [SerializeField] private int foodGatherRate = 1;
    [SerializeField] private EquipmentConfig miningTool;
    [SerializeField] private int goldGatherRate = 1;

    private GameObject gatheringTool;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Update()
    {
        Gathering();
        DropOff();
    }

    private void DropOff()
    {
        if (targetResource == null && dropOff != null)
        {
            if (gatheredAmount > 0)
            {
                if (unit != null && unit.Animator().GetBool("working")) unit.Animator().SetBool("working", false);
                if (unit != null && unit.Animator().GetBool("gathering")) unit.Animator().SetBool("gathering", false);
                if (unit != null && unit.Animator().GetBool("mining")) unit.Animator().SetBool("mining", false);
                unit.MoveTo(dropOff);
            }
        }
    }

    private void Gathering()
    {
        if (targetResource == null)
        {
            return;
        }

        if (isInRange)
        {
            GatherResource();
        }
        else
        {
            unit.MoveTo(targetResource);
        }
    }

    private void GatherResource()
    {
        if (gatheredAmount >= maxCarry)
        {
            ClearTargetResource(true);
            return;
        }
        if (!targetResource.HasResource())
        {
            ClearTargetResource();
            FindNearByResource();
            return;
        }
        unit.StopMoveTo();
        if (gatheringTool == null) EquipTool();


        
        switch(targetResource.ResourceType())
        {
            case CollectableResource.EResourceType.Wood:
                unit.Animator().SetBool("working", true);
                break;

            case CollectableResource.EResourceType.Food:
                if (targetResource.GetComponent<Building>() == null)
                {
                    unit.Animator().SetBool("gathering", true);
                }
                else
                {
                    unit.Animator().SetBool("working", true);
                }
                break;

            case CollectableResource.EResourceType.Gold:
                unit.Animator().SetBool("mining", true);
                break;
        }

        transform.LookAt(targetResource.transform);
    }

    private void FindNearByResource()
    {
        //print("finding nearby " + gatheringType);
        GameController gameController = FindObjectOfType<GameController>();

        CollectableResource nearest = gameController.NearbyResource(transform.position, unit.SightDistance(), gatheringType);

        if (nearest != null) lastTargetResource = nearest;
    }

    public void SetTargetResource(CollectableResource newResource, bool isAlreadyAtResource = false)
    {
        if (newResource.HasCollector && !newResource.IsCollector(this)) return;
        if (newResource != targetResource && newResource != lastTargetResource) ClearResourcesCollectors();

        unit.ClearPreviousActions();
        targetResource = newResource;
        targetResource.SetCollector(this);

        SetTargetDropOffPoint(unit.Faction.ClosestResourceDropPoint(targetResource));
        if (dropOff == null) Debug.LogError(name + " has no drop off point.");
        unit.HUD_StatusUpdate();

        isInRange = isAlreadyAtResource;
        //print(name + " set new target= " + targetResource);
    }

    public void SetLastTargetToCurrectTargetResource()
    {
        if (lastTargetResource != null && targetResource == null)
        {
            //print(name + "setting to last target = " + lastTargetResource);
            targetResource = lastTargetResource;
            lastTargetResource = null;

            SetTargetDropOffPoint(unit.Faction.ClosestResourceDropPoint(targetResource));
            unit.HUD_StatusUpdate();
            isInRange = false;
        }

        
    }

    public void SetTargetDropOffPoint(Building building)
    {
        dropOff = building;
        unit.HUD_StatusUpdate();
    }    

    public void ClearTargetResource(bool rememberForLater = false)
    {

        if (targetResource != null)
        {
            if (rememberForLater) lastTargetResource = targetResource;
            targetResource = null;
        }
        else if (targetResource == null && rememberForLater)
        {
            FindNearByResource();
        }    
        if (unit != null)
        {
            unit.Animator().SetBool("working", false);
            unit.Animator().SetBool("gathering", false);
            unit.Animator().SetBool("mining", false);
            unit.HUD_StatusUpdate();
        }
        else Debug.LogError(gameObject.name + " Gatherer's unit referance missing.");

        UnequipTool();
        isInRange = false;
    }

    public void ClearDropOffPoint()
    {
        if (dropOff != null) dropOff = null;
        unit.HUD_StatusUpdate();
        isInRange=false;
    }

    public void ClearResourcesCollectors()
    {
        if (targetResource != null)
        {
            targetResource.ClearCollector();
        }
        if (lastTargetResource != null)
        {
            lastTargetResource.ClearCollector();
        }
    }

    public bool IsInRange()
    {
        /*if (targetResource != null)
        {
            float distance = Vector3.Distance(transform.position, targetResource.transform.position);
            return distance < gatherRange;
        }
        else return false;*/

        return isInRange;
    }

    private void EquipTool()
    {
        if (targetResource != null)
        {
            CollectableResource.EResourceType type = targetResource.ResourceType();

            switch(type)
            {
                case CollectableResource.EResourceType.Wood:
                    if (woodCuttingTool != null) gatheringTool = woodCuttingTool.Spawn(unit);
                    gatherRate = woodGatherRate;
                    break;

                case CollectableResource.EResourceType.Food:
                    if (farmingTool != null && targetResource.GetComponent<Building>() != null) gatheringTool = farmingTool.Spawn(unit);
                    gatherRate = foodGatherRate;
                    break;

                case CollectableResource.EResourceType.Gold:
                    if (miningTool != null) gatheringTool = miningTool.Spawn(unit);
                    gatherRate = goldGatherRate;
                    break;
            }
        }
    }

    private void UnequipTool()
    {
        if (gatheringTool != null)
        {
            Destroy(gatheringTool);
            gatheringTool = null;
        }
    }

    private void GatherEffect()
    {
        if (gatheredAmount >= maxCarry) return;

        if (targetResource != null &&  targetResource.ResourceType() != gatheringType)
        {
            gatheringType = targetResource.ResourceType();
            gatheredAmount = 0;
        }
        
        if (gatheredAmount + gatherRate > maxCarry)
        {
            gatheredAmount += targetResource.Gather(maxCarry - gatheredAmount);
            ClearTargetResource(true);
        }
        else
        {
            gatheredAmount += targetResource.Gather(gatherRate);
        }

        //if (targetResource == null) ClearTargetResource(true);
        unit.HUD_StatusUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dropOff == null) return;

        if (other.gameObject.GetComponent<Building>() == dropOff)
        {
            dropOff = null;

            if (faction == null) SetFaction();
            faction.AddToStockpile(gatheringType, gatheredAmount);
            gatheredAmount = 0;
            unit.StopMoveTo();
            unit.HUD_StatusUpdate();

            if (lastTargetResource != null && lastTargetResource.HasResource())
            {
                SetLastTargetToCurrectTargetResource();
            }
            else if (lastTargetResource != null && !lastTargetResource.HasResource() && lastTargetResource.IsAFarm())
            {
                BuildingConstructor buildingConstructor = GetComponent<BuildingConstructor>();
                Building farm = lastTargetResource.GetComponent<Building>();

                if (buildingConstructor != null && farm != null)
                {
                    buildingConstructor.SetBuildTarget(farm);
                    lastTargetResource = null;
                }
            }
            else
            {
                unit.TakeAStepBack();
            }
            
        }

        if (other.gameObject.GetComponent<CollectableResource>() == targetResource)
        {
            isInRange = true;
        }
    }

    public int GatheredResourcesAmount()
    {
        return gatheredAmount;
    }

    public CollectableResource.EResourceType GatheredResourceType()
    {
        return gatheringType;
    }

    private void SetFaction()
    {
        faction = FindObjectOfType<GameController>().GetFaction(unit.PlayerNumber());
    }

    public bool hasResourceTarget { get { return targetResource != null; } }
    
    public bool hasLastTargetResource { get { return lastTargetResource != null; } }

    public bool HasDropOffTarget() { return dropOff != null; }

    public CollectableResource CurrentTarget { get {  return targetResource; } }

    public CollectableResource lastTaret { get { return lastTargetResource; } }

}
// Writen by Lukasz Dziedziczak