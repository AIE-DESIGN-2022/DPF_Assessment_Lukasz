using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    private Faction faction;
    private int playerNumber;
    public List<Unit> workers = new List<Unit>();
    public List<Unit> foodGatherers = new List<Unit>();
    public List<Unit> woodGatherers = new List<Unit>();
    public List<Unit> goldGatherers = new List<Unit>();
    private UnitProducer townCenter;
    private UnitProducer townCenter2;
    private UnitProducer townCenter3;
    public int foodGathererTarget = 3;
    public int woodGathererTarget = 3;
    public int goldGathererTarget = 1;
    private List<CollectableResource> collectableResources = new List<CollectableResource>();
    private BuildingConstructor buildingConstructor;
    private UnitProducer barracks;
    private UnitProducer university;
    private LayerMask terrainMask; // contains only the terrain layer
    private LayerMask selectableMask; // contains only the terrain and selectable layers
    [SerializeField] private float distanceFromOtherSelectables = 2.5f;
    private List<Building> farms = new List<Building>();
    private Vector3 offScreenLocation = new Vector3(-100, -100, -100);
    private float tick = 0;
    private float tickRate = 0.5f;
    private IEnumerator assignWorkersRoles;
    private IEnumerator setFinishedGatherersBackToWorker;
    public List<Unit.EUnitType> unbuiltWave = new List<Unit.EUnitType>();
    public List<Unit> currentWave = new List<Unit>();
    private int waveNumber = 0;
    private float waveTimer = Mathf.Infinity;
    private float waveRate = 120;
    private bool buildNextOutpost = false;

    private void Start()
    {
        faction = GetComponent<Faction>();
        playerNumber = faction.PlayerNumber();
        Faction.newUnitCreated += NewUnit;
        Faction.newBuildingCreated += NewBuilding;
        CollectableResource.onResourceDepleted += ResourceConsumed;
        townCenter = faction.buildings[0].GetComponent<UnitProducer>();
        collectableResources = townCenter.Building.GetResourcesInSight();
        terrainMask |= (1 << LayerMask.NameToLayer("Terrain"));
        selectableMask |= (1 << LayerMask.NameToLayer("Selectable"));
    }

    private void Update()
    {
        Tick();

        PlaceBuildings();
    }

    private void Tick()
    {
        tick += Time.deltaTime;
        waveTimer += Time.deltaTime;
        if (tick > tickRate)
        {
            tick = 0;
            WorkerUpkeep();
            BuildingsUpkeep();
            ArmyUpkeep();
        }
    }

    private void ArmyUpkeep()
    {
        if (waveTimer > waveRate && 
            barracks != null && 
            barracks.GetComponent<Building>().ConstructionComplete() && 
            unbuiltWave.Count == 0 && 
            currentWave.Count == 0)
        {
            waveTimer = 0;
            GenerateNextWave();
        }

        if (unbuiltWave.Count > 0)
        {
            if (faction.CanAfford(unbuiltWave[0]))
            {
                foreach (Unit.EUnitType newType in unbuiltWave.ToArray())
                {
                    if (faction.CanAfford(newType))
                    {
                        barracks.AddToQue(newType);
                        unbuiltWave.Remove(newType);
                    }
                }
            }    
        }
    }

    private void PlaceBuildings()
    {
        List<Building> unfinishedBuildings = new List<Building>();

        foreach (Building building in faction.buildings)
        {
            if (building.BuildState() == Building.EBuildState.Placing || building.BuildState() == Building.EBuildState.PlacingBad)
            {
                unfinishedBuildings.Add(building);
            }
        }

        if (unfinishedBuildings.Count > 0)
        {
            foreach (Building building in unfinishedBuildings)
            {
                Vector3 newLocation = RandomPointOnTerrain();

                if (IsNear(newLocation, townCenter.GetComponent<Building>()))
                {
                    building.transform.position = newLocation;
                    if (building.BuildState() == Building.EBuildState.Placing && !IsNearOtherSelectables(building))
                    {
                        building.SetBuildState(Building.EBuildState.Building);
                    }
                    else
                    {
                        building.transform.position = offScreenLocation;
                    }
                }
            }
        } 
    }

    private void BuildingsUpkeep()
    {
        if (buildingConstructor != null && !buildingConstructor.HasBuildTarget())
        {
            BuildBarracks();
            BuildUniversity();
            BuildNextOutpost();

            Building nextBuilding = NextBuildingNeedConstruction();
            if (nextBuilding != null) buildingConstructor.SetBuildTarget(nextBuilding);
        }
    }

    private void BuildBarracks()
    {
        if (barracks == null && faction.CanAfford(Building.EBuildingType.Barraks))
        {
            Building newBuilding = faction.SpawnBuilding(Building.EBuildingType.Barraks, Constructors());
            barracks = newBuilding.GetComponent<UnitProducer>();
        }
    }

    private void BuildUniversity()
    {
        if (barracks != null && university == null && faction.CanAfford(Building.EBuildingType.University))
        {
            Building newBuilding = faction.SpawnBuilding(Building.EBuildingType.University, Constructors());
            university = newBuilding.GetComponent<UnitProducer>();
        }
    }

    private void BuildFarmLogic(Unit newFarmer)
    {
        bool needMoreFarms = true;

        if (farms.Count > 0)
        {
            foreach (Building building in farms)
            {
                if (building.BuildState() == Building.EBuildState.Placing ||
                    building.BuildState() == Building.EBuildState.PlacingBad)
                {
                    needMoreFarms = false;
                    continue;
                }

                else if (building.BuildState() == Building.EBuildState.Building)
                {
                    needMoreFarms = false;
                    newFarmer.GetComponent<BuildingConstructor>().SetBuildTarget(building);
                }

                else if (building.BuildState() == Building.EBuildState.Complete)
                {
                    CollectableResource farm = building.GetComponent<CollectableResource>();

                    if (farm != null && !farm.HasCollector)
                    {
                        newFarmer.SetTarget(farm);
                        workers.Remove(newFarmer);
                        foodGatherers.Add(newFarmer);
                    }
                }
            }
            if (farms.Count >= foodGathererTarget) needMoreFarms = false;
        }

        if (needMoreFarms)
        {
            BuildAFarm();
        }
    }

    private void BuildAFarm()
    {
        List<BuildingConstructor> constructors = new List<BuildingConstructor>();
        Building newFarm = faction.SpawnBuilding(Building.EBuildingType.Farm, constructors);
        farms.Add(newFarm);
    }

    private void WorkerUpkeep()
    {
        BuildWorker();

        if (assignWorkersRoles == null)
        {
            assignWorkersRoles = AssignWorkersRoles();
            StartCoroutine(assignWorkersRoles);
        }


        if (setFinishedGatherersBackToWorker == null)
        {
            setFinishedGatherersBackToWorker = SetFinishedGatherersBackToWorker();
            StartCoroutine(setFinishedGatherersBackToWorker);
        }
    }

    private IEnumerator AssignWorkersRoles()
    {
        if (workers.Count > 0)
        {
            foreach(Unit worker in workers.ToArray())
            {
                if (worker.GetComponent<BuildingConstructor>().HasBuildTarget()) continue;

                if (foodGatherers.Count < foodGathererTarget)
                {
                    if (HaveResourceInSight(CollectableResource.EResourceType.Food))
                    {
                        AssignWorkerJob(worker, ResourceInSight(CollectableResource.EResourceType.Food));
                    }
                    else
                    {
                        BuildFarmLogic(worker);
                    }
                }

                else if (woodGatherers.Count < woodGathererTarget)
                {
                    if (HaveResourceInSight(CollectableResource.EResourceType.Wood)) AssignWorkerJob(worker, ResourceInSight(CollectableResource.EResourceType.Wood));
                }

                else if (goldGatherers.Count < goldGathererTarget)
                {
                    if (HaveResourceInSight(CollectableResource.EResourceType.Gold)) AssignWorkerJob(worker, ResourceInSight(CollectableResource.EResourceType.Gold));
                }

                else if (buildingConstructor == null)
                {
                    workers.Remove(worker);
                    buildingConstructor = worker.GetComponent<BuildingConstructor>();
                }

                yield return null;

            }
        }

        assignWorkersRoles = null;
    }

    private IEnumerator SetFinishedGatherersBackToWorker()
    {
        if (foodGatherers.Count > 0)
        {
            foreach (Unit unit in foodGatherers.ToArray())
            {
                if (!unit.GetComponent<ResourceGatherer>().HasResourceTarget() && !unit.GetComponent<ResourceGatherer>().HasDropOffTarget())
                {
                    unit.ClearPreviousActions();
                    foodGatherers.Remove(unit);
                    if (!workers.Contains(unit)) workers.Add(unit);

                    yield return null;
                }

                else if (!unit.IsWorking())
                {
                    unit.ClearPreviousActions();
                    foodGatherers.Remove(unit);
                    if (!workers.Contains(unit)) workers.Add(unit);
                    unit.MoveTo(townCenter.transform.position);
                    yield return null;
                }
            }
        }

        if (woodGatherers.Count > 0)
        {
            foreach (Unit unit in woodGatherers.ToArray())
            {
                if (!unit.GetComponent<ResourceGatherer>().HasResourceTarget() && !unit.GetComponent<ResourceGatherer>().HasDropOffTarget())
                {
                    unit.ClearPreviousActions();
                    woodGatherers.Remove(unit);
                    if (!workers.Contains(unit)) workers.Add(unit);
                }

                yield return null;
            }
        }

        if (goldGatherers.Count > 0)
        {
            foreach (Unit unit in goldGatherers.ToArray())
            {
                if (!unit.GetComponent<ResourceGatherer>().HasResourceTarget() && !unit.GetComponent<ResourceGatherer>().HasDropOffTarget())
                {
                    unit.ClearPreviousActions();
                    goldGatherers.Remove(unit);
                    if (!workers.Contains(unit)) workers.Add(unit);
                }
            }

            yield return null;
        }

        setFinishedGatherersBackToWorker = null;
    }

    private void BuildWorker()
    {
        if (TotalWorkers() < totalWorkerTarget && faction.CanAfford(Unit.EUnitType.Worker))
        {
            townCenter.AddToQue(Unit.EUnitType.Worker);
        }
    }

    private void NewBuilding(Building newBuilding)
    {
        if (newBuilding.PlayerNumber() == playerNumber)
        {
            if (newBuilding.BuildingType() == Building.EBuildingType.Barraks)
            {
                foodGathererTarget = 6;
            }


            if (newBuilding.BuildingType() == Building.EBuildingType.University && townCenter2 == null)
            {
                buildNextOutpost = true;
                goldGathererTarget = 3;
            }
            
            if (newBuilding.BuildingType() == Building.EBuildingType.TownCenter)
            {
                List<CollectableResource> newResources = newBuilding.GetResourcesInSight();

                foreach(CollectableResource resource in newResources)
                {
                    collectableResources.Add(resource);
                }
            }
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
            else
            {
                currentWave.Add(newUnit);
            }

        }
    }

    public void SelectableDied(Selectable selectable)
    {
        Unit unit = selectable.GetComponent<Unit>();
        if (unit != null)
        {
            if (unit.UnitType() == Unit.EUnitType.Worker)
            {
                if (workers.Contains(unit)) workers.Remove(unit);
                else if (foodGatherers.Contains(unit)) foodGatherers.Remove(unit);
                else if (woodGatherers.Contains(unit)) woodGatherers.Remove(unit);
                else if (goldGatherers.Contains(unit)) goldGatherers.Remove(unit);
                else if (buildingConstructor == unit) buildingConstructor = null;
            }
            else
            {
                if (currentWave.Contains(unit)) currentWave.Remove(unit);
            }
        }
        else
        {
            Building building = selectable.GetComponent<Building>();

            if (building != null)
            {
                if (building == townCenter.GetComponent<Building>()) townCenter = null;
                else if (building == barracks.GetComponent<Building>()) barracks = null;
                else if (building == university.GetComponent<Building>()) university = null;
            }
        }
    }

    private void AssignWorkerJob(Unit worker, CollectableResource newResource)
    {
        switch (newResource.ResourceType())
        {
            case CollectableResource.EResourceType.Food:
                workers.Remove(worker);
                if(!foodGatherers.Contains(worker)) foodGatherers.Add(worker);
                worker.SetTarget(newResource);
                break;

            case CollectableResource.EResourceType.Wood:
                workers.Remove(worker);
                if (!woodGatherers.Contains(worker)) woodGatherers.Add(worker);
                worker.SetTarget(newResource);
                break;

            case CollectableResource.EResourceType.Gold:
                workers.Remove(worker);
                if (!goldGatherers.Contains(worker)) goldGatherers.Add(worker);
                worker.SetTarget(newResource);
                break;
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
        if (buildingConstructor != null) workerCount++;

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

    private List<BuildingConstructor> Constructors()
    {
        List<BuildingConstructor> constructors = new List<BuildingConstructor>();

        constructors.Add(buildingConstructor);

        return constructors;
    }

    private Vector3 RandomPointOnTerrain()
    {
        float mapsize = FindObjectOfType<GameController>().CameraController().MapSize();
        float randomX = UnityEngine.Random.Range(0, mapsize);
        float randomZ = UnityEngine.Random.Range(0, mapsize);
        float y = TerrainLevel(randomX, randomZ);

        return new Vector3(randomX, y, randomZ);
    }

    private bool IsNear(Vector3 newPosition, Selectable selectable)
    {
        float distance = Vector3.Distance(selectable.transform.position, newPosition);
        return distance < selectable.SightDistance();
    }

    private float TerrainLevel(float x, float z)
    {
        Vector3 origin = new Vector3(x, 30, z);
        Vector3 direction = transform.up * -1;
        RaycastHit hit;
        Physics.Raycast(origin, direction, out hit, 50.0f, terrainMask);
        return hit.point.y;
    }

    public void ResourceConsumed(CollectableResource collectableResource)
    {
        if (collectableResources.Contains(collectableResource))
        {
            collectableResources.Remove(collectableResource);
        }
    }

    public bool IsNearOtherSelectables(Selectable selectable)
    {
        RaycastHit[] hits = Physics.SphereCastAll(selectable.transform.position, distanceFromOtherSelectables, transform.up, 20, selectableMask);

        List<Selectable> selectables = new List<Selectable>();

        foreach (RaycastHit hit in hits)
        {
            Selectable hitSelectable = hit.transform.GetComponent<Selectable>();

            if (hitSelectable != null && hitSelectable != selectable)
            {
                selectables.Add(hitSelectable);
            }
        }

        return selectables.Count > 0;
    }

    private Building NextBuildingNeedConstruction()
    {
        foreach(Building building in faction.buildings)
        {
            if (building.BuildingType() == Building.EBuildingType.Farm) continue;

            if (building.BuildState() == Building.EBuildState.Building) return building;

            if (building.BuildState() == Building.EBuildState.Complete && !building.GetComponent<Health>().IsFull()) return building;
        }

        return null;
    }

    private void GenerateNextWave()
    {
        waveNumber++;

        int numberOfUnitsToGenerate = 3 + (waveNumber - 1);

        for (int index = 0 ; index < numberOfUnitsToGenerate; index++)
        {
            int randomNumber = UnityEngine.Random.Range(0, 100);

            if (randomNumber > 50)
            {
                unbuiltWave.Add(Unit.EUnitType.Ranged);
            }
            else
            {
                unbuiltWave.Add(Unit.EUnitType.Melee);
            }
        }


    }

    private void BuildNextOutpost()
    {
        if (buildNextOutpost && faction.CanAfford(Building.EBuildingType.TownCenter))
        {
            print("Building next outpost");
            Vector3 nextLocation = FindObjectOfType<GameController>().GetNearestOutpostLocation(townCenter.transform.position);

            if (nextLocation != new Vector3())
            {
                Building newTownCenter = faction.SpawnBuilding(Building.EBuildingType.TownCenter, Constructors());
                newTownCenter.transform.position = nextLocation;
                newTownCenter.SetBuildState(Building.EBuildState.Building);

                if (townCenter2 == null) townCenter2 = newTownCenter.GetComponent<UnitProducer>();
                else if (townCenter3 == null) townCenter3 = newTownCenter.GetComponent<UnitProducer>();

                buildNextOutpost = false;
            }
        }

        
    }

    private int totalWorkerTarget { get {return foodGathererTarget + woodGathererTarget + goldGathererTarget + 1;} }

}
