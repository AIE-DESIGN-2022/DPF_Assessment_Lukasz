// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class CollectableResource : Selectable
{
    [SerializeField] private EResourceType resourceType;
    [SerializeField] private int amount;

    private int currentAmount;

    private new void Start()
    {
        base.Start();
        currentAmount = amount;
        GetComponent<NavMeshObstacle>().carving = true;
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
}
// Writen by Lukasz Dziedziczak