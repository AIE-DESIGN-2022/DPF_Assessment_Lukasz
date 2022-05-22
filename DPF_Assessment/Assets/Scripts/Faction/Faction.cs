using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    [SerializeField] private EFaction _faction;
    [SerializeField] private int _playerNumber;
    [Header("Resource Stockpile")]
    [SerializeField] private int _wood;
    [SerializeField] private int _food;
    [SerializeField] private int _gold;

    private FactionConfig _config;
    private List<Unit> _units = new List<Unit>();
    private List<Building> _buildings = new List<Building>();
    private GameController _gameController;

    public enum EFaction
    {
        Human,
        Goblin
    }

    private void Awake()
    {
        LoadFactoryConfig();
    }

    // Start is called before the first frame update
    void Start()
    {
        NameFaction();
        SetupChildren();
    }

    private void SetupChildren()
    {
        Selectable[] children = GetComponentsInChildren<Selectable>();
        foreach (Selectable child in children)
        {
            child.SetPlayerNumber(_playerNumber);
            
            Unit unit = child.GetComponent<Unit>();
            if (unit != null) _units.Add(unit);
            else
            {
                Building building = child.GetComponent<Building>();
                if (building != null) _buildings.Add(building);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public EFaction FactionType()
    {
        return _faction;
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
        _newUnit.SetPlayerNumber(_playerNumber);
        _newUnit.transform.parent = transform;
        _units.Add(_newUnit);
        return _newUnit;
    }

    public FactionConfig Config()
    {
        if (_config != null)
        {
            return _config;
        }
        else
        {
            Debug.LogError(gameObject.name + " missing FactionConfig");
            return null;
        }
    }

    public Building ClosestResourceDropPoint(CollectableResource collectableResource)
    {
        List<Building> _dropPoints = new List<Building>();

        if (_buildings.Count <= 0)
        {
            Debug.LogError(gameObject.name + " has no list of buildings.");
            return null;
        }

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

    public int StockpileAmount(CollectableResource.EResourceType _type)
    {
        switch(_type)
        {
            case CollectableResource.EResourceType.Food:
                return _food;

            case CollectableResource.EResourceType.Wood:
                return _wood;

            case CollectableResource.EResourceType.Gold:
                return _gold;

            default:
                return 0;
        }    
    }

    public void AddToStockpile(CollectableResource.EResourceType _type, int _amount)
    {
        switch (_type)
        {
            case CollectableResource.EResourceType.Food:
                _food += _amount;
                break;

            case CollectableResource.EResourceType.Wood:
                _wood += _amount;
                break;

            case CollectableResource.EResourceType.Gold:
                _gold += _amount;
                break;

            default:
                Debug.Log("Unable to add to Faction Stockpile, unknown resource type.");
                break;
        }

        if (_gameController.IsPlayerFaction(_playerNumber)) _gameController.HUD_Manager().Resource_HUD().UpdateResources();
    }

    public void RemoveFromStockpile(CollectableResource.EResourceType _type, int _amount)
    {
        switch (_type)
        {
            case CollectableResource.EResourceType.Food:
                _food -= _amount;
                break;

            case CollectableResource.EResourceType.Wood:
                _wood -= _amount;
                break;

            case CollectableResource.EResourceType.Gold:
                _gold -= _amount;
                break;

            default:
                Debug.Log("Unable to add to Faction Stockpile, unknown resource type.");
                break;
        }

        if (_gameController.IsPlayerFaction(_playerNumber)) _gameController.HUD_Manager().Resource_HUD().UpdateResources();
    }


    public void SetGameController(GameController _controller)
    {
        _gameController =  _controller;
    }

    public void SetFactionConfig(FactionConfig _newConfig)
    {
        Debug.Log(name + " setting new config as " + _newConfig.ToString());
        _config = _newConfig;
    }    

    public void Death(Selectable _deadSelectable)
    {
        Unit _unit = _deadSelectable.GetComponent<Unit>();
        if (_unit != null) _units.Remove(_unit);
        else
        {
            Building _building = _deadSelectable.GetComponent<Building>();
            if (_building != null) _buildings.Remove(_building);
        }
    }

    public bool CanAfford(Unit.EUnitType _newUnit)
    {
        FactionConfig.FactionUnit _unitConfig = _config.UnitConfig(_newUnit);

        return _food >= _unitConfig.foodCost && _wood >= _unitConfig.woodCost && _gold >= _unitConfig.goldCost;
    }

    public bool CanAfford(Building.EBuildingType _newBuilding)
    {
        FactionConfig.FactionBuilding _buildingConfig = _config.BuildingConfig(_newBuilding);

        return _food >= _buildingConfig.foodCost && _wood >= _buildingConfig.woodCost && _gold >= _buildingConfig.goldCost;
    }
    
    public void SubtractFromStockpileCostOf(Unit.EUnitType _newUnit)
    {
        FactionConfig.FactionUnit _unitConfig = _config.UnitConfig(_newUnit);

        _food -= _unitConfig.foodCost;
        _wood -= _unitConfig.woodCost;
        _gold -= _unitConfig.goldCost;

        if (_gameController.IsPlayerFaction(_playerNumber)) _gameController.HUD_Manager().Resource_HUD().UpdateResources();
    }

    public void SubtractFromStockpileCostOf(Building.EBuildingType _newBuilding)
    {
        FactionConfig.FactionBuilding _buildingConfig = _config.BuildingConfig(_newBuilding);

        _food -= _buildingConfig.foodCost;
        _wood -= _buildingConfig.woodCost;
        _gold -= _buildingConfig.goldCost;

        if (_gameController.IsPlayerFaction(_playerNumber)) _gameController.HUD_Manager().Resource_HUD().UpdateResources();
    }

    private void LoadFactoryConfig()
    {
        FactionConfig[] _loadedConfigs = Resources.LoadAll<FactionConfig>("Factions/");
        //Debug.Log(_loadedConfigs.Length + " FactoryConfigs loaded for " + name);
        foreach (var _loadedConfig in _loadedConfigs)
        {
            //Debug.Log("Checking loadedConfig " + _loadedConfig.ToString());
            if (_loadedConfig.Faction() == _faction)
            {
                //Debug.Log("Found match: loadedConfig " + _loadedConfig.Faction().ToString() + " and " + _faction.ToString());
                _config = _loadedConfig;
            }

        }

        if (_config == null) Debug.LogError(name + " could not load FactionConfig.");
        if (_config.Faction() != _faction) Debug.LogError(name + "has FactionConfig mismatch.");
        
    }
}
