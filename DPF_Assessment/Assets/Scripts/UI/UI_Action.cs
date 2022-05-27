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
        Blank,
        StancePassive,
        StanceDefensive,
        StanceOffensive,
        Patrol
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
                BuildButton(_buildableUnit, _unitProducer);
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
            BuildButton(EButtonType.Build, _selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, _selectedUnits);
        }

        BuildButton(EButtonType.Blank, _selectedUnits);

        if (AnyAreAttackers(_selectedUnits))
        {
            BuildButton(EButtonType.Patrol, _selectedUnits); //this will be patrol button

            BuildButton(EButtonType.StancePassive, _selectedUnits);
            BuildButton(EButtonType.StanceDefensive, _selectedUnits);
            BuildButton(EButtonType.StanceOffensive, _selectedUnits);
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

        BuildButton(EButtonType.Back, _selectedUnits);

        //make buttons for each building.
        foreach (Building.EBuildingType _buildingType in _constructableBuildings)
        {
            BuildButton(_buildingType, _constructionTeam);
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

            case EButtonType.StancePassive:
                ChangeStanceOnUnits(_units, Unit.EUnitStance.Passive);
                break;

            case EButtonType.StanceDefensive:
                ChangeStanceOnUnits(_units, Unit.EUnitStance.Defensive);
                break;

            case EButtonType.StanceOffensive:
                ChangeStanceOnUnits(_units, Unit.EUnitStance.Offensive);
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

    private bool AnyAreAttackers(List<Unit> units)
    {
        foreach (Unit _unit in units)
        {
            if (_unit.GetComponent<Attacker>() != null) return true;
        }
        return false;
    }

    private void ChangeStanceOnUnits(List<Unit> units, Unit.EUnitStance newStance)
    {
        foreach (Unit _unit in units)
        {
            _unit.ChangeUnitStance(newStance);
        }

        UpdateUnitStances();
    }

    private void BuildButton(EButtonType type, List<Unit> _selectedUnits)
    {
        UI_Action_Button _buildButton = Instantiate(_actionButtonPrefab, transform);
        _buildButton.SetupButton(type, _selectedUnits);
        _actionButtons.Add(_buildButton);
    }

    private void BuildButton(Unit.EUnitType unitType, UnitProducer unitProducer)
    {
        UI_Action_Button _newButton = Instantiate(_actionButtonPrefab, transform);
        _newButton.SetupButton(unitProducer, unitType, _faction.Config().Icon(unitType));
        _actionButtons.Add(_newButton);
    }

    private void BuildButton(Building.EBuildingType buildingType, List<BuildingConstructor> _constructionTeam)
    {
        UI_Action_Button _newButton = Instantiate(_actionButtonPrefab, transform);
        _newButton.SetupButton(_faction, buildingType, _faction.Config().Icon(buildingType), _constructionTeam);
        _actionButtons.Add(_newButton);
    }

    public void UpdateUnitStances()
    {
        if (_actionButtons.Count > 0)
        {
            foreach (UI_Action_Button action_Button in _actionButtons)
            {
                action_Button.UpdateUnitStance();
            }
        }
    }
}
// Writen by Lukasz Dziedziczak