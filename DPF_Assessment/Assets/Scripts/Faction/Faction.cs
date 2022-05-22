using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    [SerializeField] private EFaction _faction;
    [SerializeField] private int _playerNumber;
    [SerializeField] private FactionConfig _config;

    private int _wood;
    private int _food;
    private int _gold;

    private List<Unit> _units;
    private List<Building> _buildings;

    public enum EFaction
    {
        Human,
        Goblin
    }

    // Start is called before the first frame update
    void Start()
    {
        NameFaction();
        SetChildrenPlayerNumber();
        _config = FindObjectOfType<GameController>().GetFactionConfig(_faction);
    }

    private void SetChildrenPlayerNumber()
    {
        Selectable[] children = GetComponentsInChildren<Selectable>();
        foreach (Selectable child in children)
        {
            child.SetPlayerNumber(_playerNumber);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupFaction(EFaction _new, int _playerNo)
    {
        _faction = _new;
        _playerNumber = _playerNo;
        NameFaction();
        
    }

    private void NameFaction()
    {
        gameObject.name = "Faction(" + _playerNumber + ")_" + _faction.ToString();
    }

    public int PlayerNumber() { return _playerNumber; }

    public Unit SpawnUnit(Unit.EUnitType _newUnitType, Transform _spawnLocation)
    {
        Unit _newUnit = Instantiate(_config.GetUnitPrefab(_newUnitType), _spawnLocation.position, _spawnLocation.rotation);
        _newUnit.transform.parent = transform;
        _units.Add(_newUnit);
        return _newUnit;
    }

    public FactionConfig Config()
    {
        return _config;
    }

    public Building ClosestResourceDropPoint(CollectableResource collectableResource)
    {
        List<Building> _dropPoints = new List<Building>();

        foreach (Building building in _buildings)
        {
            if (building.IsResourceDropPoint()) _dropPoints.Add(building);
        }

        Building _closest = null;
        float _smallestDistance = Mathf.Infinity;

        foreach (Building _dropPoint in _dropPoints)
        {
            float _distance = Vector3.Distance(_dropPoint.transform.position, collectableResource.transform.position);
            if (_distance < _smallestDistance)
            {
                _smallestDistance = _distance;
                _closest = _dropPoint;
            }
        }

        return _closest;
    }
}
