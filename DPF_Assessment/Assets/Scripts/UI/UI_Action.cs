// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Action : MonoBehaviour
{
    private List<UI_Action_Button> _actionButtons = new List<UI_Action_Button>();
    private UI_Action_Button _actionButtonPrefab;
    private Faction _faction;

    public enum EButtonType
    {
        Build,
        Back,
        Blank
    }

    private void Awake()
    {
        _actionButtonPrefab = (UI_Action_Button)Resources.Load<UI_Action_Button>("HUD_Prefabs/UI_Action_Button");
    }

    private void Start()
    {
        if (_actionButtonPrefab == null)
        {
            Debug.LogError(name + " is missing ActionButtonPrefab.");
            return;
        }
    }

    public void SetFaction(Faction _newFaction)
    {
        _faction = _newFaction;
    }

    public void BuildingSelected(Building _selectedBuilding)
    {
        if (_faction.PlayerNumber() != _selectedBuilding.PlayerNumber()) return;

        UnitProducer _unitProducer = _selectedBuilding.GetComponent<UnitProducer>();
        if (_unitProducer != null)
        {
            //print("Action UI sees unitProducer");
            List<Unit.EUnitType> _buildableUnits = _unitProducer.GetListOfBuildableUnits();

            foreach (Unit.EUnitType _buildableUnit in _buildableUnits)
            {
                UI_Action_Button _newButton = Instantiate(_actionButtonPrefab, transform);
                _newButton.SetupButton(_unitProducer, _buildableUnit, _faction.Config().Icon(_buildableUnit));
                _actionButtons.Add(_newButton);
            }
        }
    }

    public void UnitSelected(Unit _selectedUnit)
    {
        if (_faction.PlayerNumber() != _selectedUnit.PlayerNumber()) return;

        List<Unit> _newTeam = new List<Unit>();
        _newTeam.Add(_selectedUnit);
        UnitsSelected(_newTeam);
    }

    public void UnitsSelected(List<Unit> _selectedUnits)
    {
        Clear();
        if (AnyAreBuildingConstructors(_selectedUnits))
        {
            UI_Action_Button _buildButton = Instantiate(_actionButtonPrefab, transform);
            _buildButton.SetupButton(EButtonType.Build, _selectedUnits);
            _actionButtons.Add(_buildButton);
        }
        else
        {
            // build a blank button in it's place
        }
    }

    public void BuildButtonPushed(List<Unit> _selectedUnits)
    {
        Clear();
        List<BuildingConstructor> _constructionTeam = new List<BuildingConstructor>();
        foreach (Unit _unit in _selectedUnits)
        {
            BuildingConstructor _constructor = _unit.GetComponent<BuildingConstructor>();
            if (_constructor) _constructionTeam.Add(_constructor);
        }

        List<Building.EBuildingType> _constructableBuildings = _faction.Config().ConstructableBuildings(_selectedUnits[0].UnitType());

        UI_Action_Button _backButton = Instantiate(_actionButtonPrefab, transform);
        _backButton.SetupButton(EButtonType.Back, _selectedUnits);
        _actionButtons.Add(_backButton);

        //make buttons for each building.
        foreach (Building.EBuildingType _buildingType in _constructableBuildings)
        {
            UI_Action_Button _newButton = Instantiate(_actionButtonPrefab, transform);
            _newButton.SetupButton(_faction, _buildingType, _faction.Config().Icon(_buildingType), _constructionTeam);
            _actionButtons.Add(_newButton);
        }
    }

    public void Clear()
    {
        if (_actionButtons.Count > 0)
        {
            foreach (UI_Action_Button _button in _actionButtons)
            {
                Destroy(_button.gameObject);
            }
            _actionButtons.Clear();
        }
    }

    public void ActionButton(EButtonType _pushedButton, List<Unit> _units)
    {
        switch (_pushedButton)
        {
            case EButtonType.Build:
                BuildButtonPushed(_units);
                break;

            case EButtonType.Back:
                UnitsSelected(_units);
                break;
        }
    }

    private bool AnyAreBuildingConstructors(List<Unit> _units)
    {
        foreach (Unit _unit in _units)
        {
            if (_unit.GetComponent<BuildingConstructor>() != null) return true;
        }
        return false;
    }
}
// Writen by Lukasz Dziedziczak