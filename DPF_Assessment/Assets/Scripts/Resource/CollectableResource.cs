// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class CollectableResource : Selectable
{
    [Header("Collectable Resource")]
    [SerializeField] private EResourceType resourceType;
    [SerializeField] private int amount = 100;

    private int currentAmount;
    private List<MeshRenderer> corn = new List<MeshRenderer>();
    private ResourceGatherer collector;

    private new void Awake()
    {
        base.Awake();
        FindFarmCorn();
    }

    private void FindFarmCorn()
    {
        if (resourceType == EResourceType.Food)
        {
            MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mesh in meshes)
            {
                if (mesh.name == "Corn")
                {
                    corn.Add(mesh);
                }
            }
        }
    }

    private new void Start()
    {
        base.Start();
        currentAmount = amount;
        GetComponent<NavMeshObstacle>().carving = true;

        if (amount < 1) Debug.LogError(name + " has no resource amount.");

        if (resourceType == EResourceType.Wood) gameController.treeCount++;

        MoveToGroundLevel();
    }

    public enum EResourceType
    {
        Wood,
        Food,
        Gold
    }

    public EResourceType ResourceType() { return resourceType; }

    public int Gather(int gatheringAmount)
    {
        int returnAmount = 0;
        if (currentAmount > 0)
        {
            if (currentAmount - gatheringAmount > 0)
            {
                returnAmount = gatheringAmount;
                currentAmount -= gatheringAmount;
            }
            else
            {
                returnAmount = currentAmount;
                currentAmount = 0;
                ResourceDepleted();
            }
        }
        HUD_StatusUpdate();
        return returnAmount;
    }

    private void ResourceDepleted()
    {
        if (resourceType == EResourceType.Wood)
        {
            MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
            Destroy(mr.gameObject);
            GameObject treeStumpPrefab = (GameObject)Resources.Load<GameObject>("Prefabs/TreeStump");
            GameObject treeStump = Instantiate(treeStumpPrefab, transform.position, transform.rotation);
            treeStump.transform.parent = transform;
        }
        else if (resourceType == EResourceType.Food)
        {
            Building farm = GetComponent<Building>();
            if (farm != null)
            {
                farm.GetComponent<Health>().NewBuilding();
                farm.SetBuildState(Building.EBuildState.Building);
                farm.FarmRebuild();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else Destroy(gameObject);

    }

    public bool HasResource() { return currentAmount > 0; }

    public int Amount() { return amount; }

    public void Status(out string newStatus1, out string newStatus2)
    {
        newStatus1 = "";
        newStatus2 = "";

        if (HasResource())
        {
            newStatus1 =  currentAmount.ToString() + " " + resourceType.ToString();
        }
        else
        {
            newStatus1 = "Depleted";
        }
    }

    public void ShowFarmCorn(bool isShowing)
    {
        if (corn.Count > 0)
        {
            foreach (MeshRenderer mesh in corn)
            {
                mesh.enabled = isShowing;
            }
        }

        if (isShowing)
        {
            currentAmount = amount;
        }
    }

    public bool IsAFarm()
    {
        bool isAFarm = false;
        Building building = GetComponent<Building>();

        if (building != null && building.BuildingType() == Building.EBuildingType.Farm) isAFarm = true;

        return isAFarm;
    }

    
    private void MoveToGroundLevel()
    {
        if (resourceType != EResourceType.Wood) return;

        LayerMask terrainMask2 = new LayerMask();
        terrainMask2 |= (1 << LayerMask.NameToLayer("Terrain"));

        Vector3 rayOrigin = transform.position;
        rayOrigin.y += 50;

        float terrainLevel = 0;

        Ray ray = new Ray(rayOrigin, transform.up * -1);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 500, terrainMask2))
        {
            terrainLevel = hit.point.y;
        }
        else
        {
            Debug.Log("no hit");
        }

        if (terrainLevel != 0)
        {
            transform.position = new Vector3(rayOrigin.x, terrainLevel, rayOrigin.z);
        }
        
    }

    public void SetCollector(ResourceGatherer newCollector)
    {
        collector = newCollector;
    }

    public bool HasCollector { get { return collector != null; } }

    public void ClearCollector()
    {
        collector = null;
    }
}
// Writen by Lukasz Dziedziczak