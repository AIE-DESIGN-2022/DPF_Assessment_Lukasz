// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class CollectableResource : Selectable
{
    [SerializeField] private EResourceType _resourceType;
    [SerializeField] private int _amount;

    private int _currentAmount;

    private new void Start()
    {
        base.Start();
        _currentAmount = _amount;
        GetComponent<NavMeshObstacle>().carving = true;
    }

    public enum EResourceType
    {
        Wood,
        Food,
        Gold
    }

    public EResourceType ResourceType() { return _resourceType; }

    public int Gather(int _gatheringAmount)
    {
        int _returnAmount = 0;
        if (_currentAmount - _gatheringAmount > 0)
        {
            _returnAmount = _gatheringAmount;
            _currentAmount -= _gatheringAmount;
        }
        else
        {
            _returnAmount = _currentAmount;
            _currentAmount = 0;
            ResourceDepleted();
        }
        HUD_StatusUpdate();
        return _returnAmount;
    }

    private void ResourceDepleted()
    {
        if (_resourceType == EResourceType.Wood)
        {
            MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
            Destroy(mr.gameObject);
            GameObject treeStumpPrefab = (GameObject)Resources.Load<GameObject>("Prefabs/TreeStump");
            GameObject treeStump = Instantiate(treeStumpPrefab, transform.position, transform.rotation);
            treeStump.transform.parent = transform;
        }
        else Destroy(gameObject);

    }

    public bool HasResource() { return _currentAmount > 0; }

    public int Amount() { return _amount; }

    public void Status(out string newStatus1, out string newStatus2)
    {
        newStatus1 = "";
        newStatus2 = "";

        if (HasResource())
        {
            newStatus1 =  _currentAmount.ToString() + " " + _resourceType.ToString();
        }
        else
        {
            newStatus1 = "Depleted";
        }
    }
}
// Writen by Lukasz Dziedziczak