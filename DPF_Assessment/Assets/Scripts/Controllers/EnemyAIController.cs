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
    public List<WorkerJob> jobs = new List<WorkerJob>();
    public List<WorkerJob> unfilledJobs = new List<WorkerJob>();

    const CollectableResource.EResourceType FOOD = CollectableResource.EResourceType.Food;
    const CollectableResource.EResourceType WOOD = CollectableResource.EResourceType.Wood;
    const CollectableResource.EResourceType GOLD = CollectableResource.EResourceType.Gold;

    [System.Serializable]
    public class WorkerJob
    {
        public CollectableResource.EResourceType jobType;
        public CollectableResource collectableResource;
        public Unit worker;
        public bool isFarm;

        public WorkerJob(CollectableResource newCollectableResource, bool isNewFarm = false)
        {
            collectableResource = newCollectableResource;
            jobType = collectableResource.ResourceType();
            isFarm = isNewFarm;
        }

        public void AssignWorker(Unit newWorker)
        {
            if (isFarm)
            {
                Building farm = collectableResource.GetComponent<Building>();
                if (farm.BuildState() == Building.EBuildState.Complete)
                {
                    worker = newWorker;
                    worker.GetComponent<ResourceGatherer>().SetTargetResource(collectableResource);
                }
                else if (farm.BuildState() == Building.EBuildState.Building)
                {
                    worker = newWorker;
                    worker.GetComponent<BuildingConstructor>().SetBuildTarget(farm);
                }
            }
            else
            {
                worker = newWorker;
                worker.GetComponent<ResourceGatherer>().SetTargetResource(collectableResource);
            }

            
        }

        public bool needsWorker { get 
            { 
                if (isFarm)
                {
                    Building farm = collectableResource.GetComponent<Building>();
                    if (farm != null && farm.BuildState() == Building.EBuildState.PlacingBad && farm.BuildState() == Building.EBuildState.Placing)
                    {
                        return false;
                    }
                    else
                    {
                        return worker == null;
                    }
                }
                else return worker == null;


            } }

        public CollectableResource.EResourceType type { get { return jobType; } }

        public CollectableResource resource {  get { return collectableResource; } }

        public void ClearWorker()
        {
            if (worker != null)
            {
                worker.ClearPreviousActions();
                worker = null;
            }
        }
    }

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
            JobUpkeep();
            WorkerUpkeep();
            BuildingsUpkeep();
            ArmyUpkeep();
        }
    }

    private void JobUpkeep()
    {
        BuildListofJobs();
        CheckJobs();
    }

    private void CheckJobs()
    {
        if (jobs.Count > 0)
        {
            foreach (WorkerJob job in jobs.ToArray())
            {
                if (job.needsWorker)
                {
                    jobs.Remove(job);
                    unfilledJobs.Add(job);
                }

                else if (job.worker != null)
                {
                    ResourceGatherer resourceGatherer = job.worker.GetComponent<ResourceGatherer>();
                    BuildingConstructor buildingConstructor = job.worker.GetComponent<BuildingConstructor>();
                    if (resourceGatherer != null &&
                        !resourceGatherer.hasResourceTarget &&
                        !resourceGatherer.hasLastTargetResource &&
                        buildingConstructor != null &&
                        !buildingConstructor.HasBuildTarget())
                    {
                        //print("found lazy worker " + name);
                        job.ClearWorker();
                        jobs.Remove(job);
                        unfilledJobs.Add(job);
                    }
                }
            }
        }
    }

    private void BuildListofJobs()
    {
        int foodJobs = NumberOfJobsOfType(FOOD);
        int woodJobs = NumberOfJobsOfType(WOOD);
        int goldJobs = NumberOfJobsOfType(GOLD);

        if (foodJobs < foodGathererTarget)
        {
            int neededJobs = foodGathererTarget - foodJobs;
            for (int i = 0; i < neededJobs; i++)
            {
                CollectableResource newResource = ResourceInSight(FOOD);
                if (newResource != null)
                {
                    collectableResources.Remove(newResource);
                    WorkerJob job = new WorkerJob(newResource);
                    unfilledJobs.Add(job);
                }
                else
                {
                    Building building = BuildFarm();
                    CollectableResource farm = building.GetComponent<CollectableResource>();
                    if (farm != null)
                    {
                        WorkerJob job = new WorkerJob(farm, true);
                        unfilledJobs.Add(job);
                    }
                }
            }
        }
        if(woodJobs < woodGathererTarget)
        {
            int neededJobs = woodGathererTarget - woodJobs;

            for (int i = 0; i < neededJobs; i++)
            {
                CollectableResource newResource = ResourceInSight(WOOD);
                if (newResource != null)
                {
                    collectableResources.Remove(newResource);
                    WorkerJob job = new WorkerJob(newResource);
                    unfilledJobs.Add(job);
                }
            }
        }

        if (goldJobs < goldGathererTarget)
        {
            int neededJobs = woodGathererTarget - woodJobs;

            for (int i = 0; i < neededJobs; i++)
            {
                CollectableResource newResource = ResourceInSight(GOLD);
                if (newResource != null)
                {
                    collectableResources.Remove(newResource);
                    WorkerJob job = new WorkerJob(newResource);
                    unfilledJobs.Add(job);
                }
            }
        }
    }

    private int NumberOfJobsOfType(CollectableResource.EResourceType jobType)
    {
        int count = 0;

        if (jobs.Count > 0)
        {
            foreach (WorkerJob job in jobs)
            {
                if(job.type == jobType) count++;
            }
        }

        if (unfilledJobs.Count > 0)
        {
            foreach (WorkerJob job in unfilledJobs)
            {
                if (job.type == jobType) count++;
            }
        }

        return count;
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

    private Building BuildFarm()
    {
        List<BuildingConstructor> constructors = new List<BuildingConstructor>();
        Building farm = faction.SpawnBuilding(Building.EBuildingType.Farm, constructors);
        farms.Add(farm);
        return farm;
    }

    private void WorkerUpkeep()
    {
        BuildWorker();
        AllocateWorkersToRoles();
        FillJobs();
    }

    private void FillJobs()
    {
        if (unfilledJobs.Count > 0)
        {
            foreach (WorkerJob job in unfilledJobs.ToArray())
            {
                if (job.needsWorker)
                {
                    Unit worker = UnemployedWorker(job.type);
                    if (worker != null)
                    {
                        job.AssignWorker(worker);

                        if (job.worker != null)
                        {
                            unfilledJobs.Remove(job);
                            jobs.Add(job);
                        }    
                    }
                }
            }
        }
    }

    private Unit UnemployedWorker(CollectableResource.EResourceType type)
    {
        List<Unit> allWorkersOfType = new List<Unit>();

        switch (type)
        {
            case FOOD:
                allWorkersOfType = foodGatherers;
                break;

            case WOOD:
                allWorkersOfType = woodGatherers;
                break;

            case GOLD:
                allWorkersOfType = goldGatherers;
                break;
        }

        if (allWorkersOfType.Count > 0)
        {
            foreach (Unit unit in allWorkersOfType)
            {
                ResourceGatherer gatherer = unit.GetComponent<ResourceGatherer>();
                BuildingConstructor constructor = unit.GetComponent<BuildingConstructor>();
                if (gatherer != null && constructor != null)
                {
                    if (!gatherer.hasResourceTarget && !gatherer.hasResourceTarget && !gatherer.HasDropOffTarget() && !constructor.HasBuildTarget())
                    {
                        return unit;
                    }
                }
            }
        }

        return null;
    }

    private void AllocateWorkersToRoles()
    {
        if (workers.Count > 0)
        {
            foreach (Unit worker in workers.ToArray())
            {
                if (foodGatherers.Count < foodGathererTarget)
                {
                    workers.Remove(worker);
                    foodGatherers.Add(worker);
                }
                else if (woodGatherers.Count < woodGathererTarget)
                {
                    workers.Remove(worker);
                    woodGatherers.Add(worker);
                }
                else if (goldGatherers.Count < goldGathererTarget)
                {
                    workers.Remove(worker);
                    goldGatherers.Add(worker);
                }
                else if (buildingConstructor == null)
                {
                    buildingConstructor = worker.GetComponent<BuildingConstructor>();
                    workers.Remove(worker);
                }
            }
        }
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
        Building farm = collectableResource.GetComponent<Building>();
        if (!farm)
        {
            CompleteJob(collectableResource);

            if (collectableResources.Contains(collectableResource))
            {
                collectableResources.Remove(collectableResource);
            }
        }
    }

    public void CompleteJob(CollectableResource collectableResource)
    {
        if (jobs.Count > 0)
        {
            foreach (WorkerJob job in jobs.ToArray())
            {
                if (job.resource == collectableResource)
                {
                    jobs.Remove(job);
                    return;
                }
            }    
        }

        if (unfilledJobs.Count > 0)
        {
            foreach (WorkerJob unfilledJob in unfilledJobs.ToArray())
            {
                if (unfilledJob.resource == collectableResource)
                {
                    unfilledJobs.Remove(unfilledJob);
                    return;
                }
            }
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
            buildNextOutpost = false;
            
            Vector3 nextLocation = FindObjectOfType<GameController>().GetNearestOutpostLocation(townCenter.transform.position);
            print("Building next outpost at " + nextLocation);
            if (nextLocation != Vector3.zero)
            {
                Building newTownCenter = faction.SpawnBuilding(Building.EBuildingType.TownCenter, Constructors());
                newTownCenter.transform.position = nextLocation;
                newTownCenter.SetBuildState(Building.EBuildState.Building);

                if (townCenter2 == null) townCenter2 = newTownCenter.GetComponent<UnitProducer>();
                else if (townCenter3 == null) townCenter3 = newTownCenter.GetComponent<UnitProducer>();

                print("have " + newTownCenter);
            }
        }

        
    }

    private int totalWorkerTarget { get {return foodGathererTarget + woodGathererTarget + goldGathererTarget + 1;} }

}
