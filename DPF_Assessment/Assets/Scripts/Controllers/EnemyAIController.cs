using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    private Faction faction;
    private int playerNumber;
    private List<Unit> workers = new List<Unit>();
    public List<Unit> foodGatherers = new List<Unit>();
    private List<Unit> woodGatherers = new List<Unit>();
    private List<Unit> goldGatherers = new List<Unit>();
    private UnitProducer townCenter;
    private int totalWorkerTarget = 8;
    private int foodGathererTarget = 3;
    private int woodGathererTarget = 3;
    private int goldGathererTarget = 1;
    private List<CollectableResource> collectableResources = new List<CollectableResource>();


    private void Start()
    {
        faction = GetComponent<Faction>();
        playerNumber = faction.PlayerNumber();
        Faction.newUnitCreated += NewUnit;
        Faction.newBuildingCreated += NewBuilding;
        townCenter = faction.buildings[0].GetComponent<UnitProducer>();
        collectableResources = townCenter.Building.GetResourcesInSight();
    }

    private void Update()
    {
        WorkerUpkeep();
    }

    private void WorkerUpkeep()
    {
        if (TotalWorkers() < totalWorkerTarget)
        {
            BuildWorker();
        }

        if (TotalGatherers() < (foodGathererTarget + woodGathererTarget + goldGathererTarget))
        {
            if (foodGatherers.Count < foodGathererTarget)
            {
                if (HaveResourceInSight(CollectableResource.EResourceType.Food)) AssignWorkerJob(ResourceInSight(CollectableResource.EResourceType.Food));
            }

            if (woodGatherers.Count < woodGathererTarget)
            {
                if (HaveResourceInSight(CollectableResource.EResourceType.Wood)) AssignWorkerJob(ResourceInSight(CollectableResource.EResourceType.Wood));
            }

            if (goldGatherers.Count < goldGathererTarget)
            {
                if (HaveResourceInSight(CollectableResource.EResourceType.Gold)) AssignWorkerJob(ResourceInSight(CollectableResource.EResourceType.Gold));
            }
        }

        if (foodGatherers.Count > 0)
        {
            foreach (Unit unit in foodGatherers.ToArray())
            {
                if (!unit.GetComponent<ResourceGatherer>().HasResourceTarget() && !unit.GetComponent<ResourceGatherer>().HasDropOffTarget())
                {
                    foodGatherers.Remove(unit);
                    workers.Add(unit);
                }
            }
        }

        if (woodGatherers.Count > 0)
        {
            foreach (Unit unit in woodGatherers.ToArray())
            {
                if (!unit.GetComponent<ResourceGatherer>().HasResourceTarget() && !unit.GetComponent<ResourceGatherer>().HasDropOffTarget())
                {
                    woodGatherers.Remove(unit);
                    workers.Add(unit);
                }
            }
        }

        if (goldGatherers.Count > 0)
        {
            foreach (Unit unit in goldGatherers.ToArray())
            {
                if (!unit.GetComponent<ResourceGatherer>().HasResourceTarget() && !unit.GetComponent<ResourceGatherer>().HasDropOffTarget())
                {
                    goldGatherers.Remove(unit);
                    workers.Add(unit);
                }
            }
        }
    }

    private void BuildWorker()
    {
        if (faction.CanAfford(Unit.EUnitType.Worker))
        {
            townCenter.AddToQue(Unit.EUnitType.Worker);
        }
    }

    private void NewBuilding(Building newBuilding)
    {
        if (newBuilding.PlayerNumber() == playerNumber)
        {

        }
    }

    private void NewUnit(Unit newUnit)
    {
        
        if (newUnit.PlayerNumber() == playerNumber)
        {
            
            if (newUnit.UnitType() == Unit.EUnitType.Worker)
            {
                workers.Add(newUnit);
            }
        }
    }

    private void AssignWorkerJob(CollectableResource newResource)
    {
        if (workers.Count > 0)
        {
            Unit unit = workers[0];

            switch (newResource.ResourceType())
            {
                case CollectableResource.EResourceType.Food:
                    workers.Remove(unit);
                    foodGatherers.Add(unit);
                    unit.SetTarget(newResource);
                    break;

                case CollectableResource.EResourceType.Wood:
                    workers.Remove(unit);
                    woodGatherers.Add(unit);
                    unit.SetTarget(newResource);
                    break;

                case CollectableResource.EResourceType.Gold:
                    workers.Remove(unit);
                    foodGatherers.Add(unit);
                    unit.SetTarget(newResource);
                    break;
            }

            //print("assigning job " + newResource.ResourceType());
        }
    }

    private int TotalWorkers()
    {
        int workerCount = 0;

        workerCount += workers.Count;
        workerCount += foodGatherers.Count;
        workerCount += woodGatherers.Count;
        workerCount += goldGatherers.Count;
        workerCount += townCenter.QueSize();

        return workerCount;
    }

    private int TotalGatherers()
    {
        int workerCount = 0;

        workerCount += foodGatherers.Count;
        workerCount += woodGatherers.Count;
        workerCount += goldGatherers.Count;

        return workerCount;
    }

    private CollectableResource ResourceInSight(CollectableResource.EResourceType resourceType)
    {
        List<CollectableResource> resources = new List<CollectableResource>();

        if (collectableResources.Count > 0)
        {
            foreach (CollectableResource resource in collectableResources)
            {
                if (resource.ResourceType() == resourceType && !resource.HasCollector) resources.Add(resource);
            }
        }

        CollectableResource closestResource = null;
        float closestDistance = Mathf.Infinity;

        if (resources.Count > 0)
        {
            foreach (CollectableResource collectableResource in resources)
            {
                float distance = Vector3.Distance(townCenter.transform.position, collectableResource.transform.position);
                if (distance < closestDistance)
                {
                    closestResource = collectableResource;
                    closestDistance = distance;
                }
            }
        }

        return closestResource;
    }

    private bool HaveResourceInSight(CollectableResource.EResourceType resourceType)
    {
        if (collectableResources.Count > 0)
        {
            foreach (CollectableResource resource in collectableResources)
            {
                if (resource.ResourceType() == resourceType && !resource.HasCollector) return true;
            }
        }

        return false;
    }

}
