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
        StancePatrol
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

        List<Building> selectedBuildings = new List<Building>();
        selectedBuildings.Add(_selectedBuilding);
        BuildingsSelected(selectedBuildings);
    }

    public void BuildingsSelected(List<Building> selectedBuildings)
    {
        UnitProducer _unitProducer = selectedBuildings[0].GetComponent<UnitProducer>();
        if (_unitProducer != null)
        {
            //print("Action UI sees unitProducer");
            List<Unit.EUnitType> _buildableUnits = _unitProducer.GetListOfBuildableUnits();

            foreach (Unit.EUnitType _buildableUnit in _buildableUnits)
            {
                BuildButton(_buildableUnit, _unitProducer);
            }
        }

        if (AnyAreTowers(selectedBuildings))
        {
            BuildBlankButton(3);

            // 4th button
            BuildButton(EButtonType.StancePassive, selectedBuildings);

            // 5th button
            BuildButton(EButtonType.StanceDefensive, selectedBuildings);
        }
    }

    private static bool AnyAreTowers(List<Building> buildings)
    {
        foreach (Building building in buildings)
        {
            if (building.BuildingType() == Building.EBuildingType.Tower) return true;
        }

        return false;
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

        // 1st button
        if (AnyAreBuildingConstructors(_selectedUnits))
        {
            BuildButton(EButtonType.Build, _selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, _selectedUnits);
        }

        // 2nd button
        BuildButton(EButtonType.Blank, _selectedUnits);

        // 3rd button
        if (AnyAreAttackers(_selectedUnits))
        {
            BuildButton(EButtonType.StancePatrol, _selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, _selectedUnits);
        }

        // 4th button
        if (AnyAreAttackers(_selectedUnits) || AnyAreHealers(_selectedUnits))
        {
            BuildButton(EButtonType.StancePassive, _selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, _selectedUnits);
        }

        // 5th button
        if (AnyAreAttackers(_selectedUnits) || AnyAreHealers(_selectedUnits))
        {
            BuildButton(EButtonType.StanceDefensive, _selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, _selectedUnits);
        }

        // 6th button
        if (AnyAreAttackers(_selectedUnits))
        {
            BuildButton(EButtonType.StanceOffensive, _selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, _selectedUnits);
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

            case EButtonType.StancePatrol:
                ChangeStanceOnUnits(_units, Unit.EUnitStance.Patrol);
                break;
        }
    }

    public void ActionButton(EButtonType _pushedButton, List<Building> buildings)
    {
        switch (_pushedButton)
        {
            /*case EButtonType.Build:
                BuildButtonPushed(_units);
                break;

            case EButtonType.Back:
                UnitsSelected(_units);
                break;*/

            case EButtonType.StancePassive:
                ChangeStanceOnTowers(buildings, Unit.EUnitStance.Passive);
                break;

            case EButtonType.StanceDefensive:
                ChangeStanceOnTowers(buildings, Unit.EUnitStance.Defensive);
                break;

            case EButtonType.StanceOffensive:
                ChangeStanceOnTowers(buildings, Unit.EUnitStance.Offensive);
                break;

            case EButtonType.StancePatrol:
                ChangeStanceOnTowers(buildings, Unit.EUnitStance.Patrol);
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

    private bool AnyAreHealers(List<Unit> selectedUnits)
    {
        foreach (Unit _unit in selectedUnits)
        {
            if (_unit.GetComponent<Healer>() != null) return true;
        }
        return false;
    }

    private void ChangeStanceOnUnits(List<Unit> units, Unit.EUnitStance newStance)
    {
        foreach (Unit _unit in units)
        {
            Healer healer = _unit.GetComponent<Healer>();

            if (healer != null && (newStance == Unit.EUnitStance.Offensive || newStance == Unit.EUnitStance.Patrol))
            {
                _unit.ChangeUnitStance(Unit.EUnitStance.Defensive);
            }
            else
            {
                _unit.ChangeUnitStance(newStance);
            }
        }
        UpdateUnitStances();
    }

    private void ChangeStanceOnTowers(List<Building> buildings, Unit.EUnitStance newStance)
    {
        foreach (Building building in buildings)
        {
            Attacker attacker = building.GetComponent<Attacker>();

            if (attacker != null && building.BuildingType() == Building.EBuildingType.Tower)
            {
                attacker.ChangeTowerStance(newStance);
            }
        }
        UpdateUnitStances();
    }

    private void BuildButton(EButtonType type, List<Unit> _selectedUnits)
    {
        UI_Action_Button _buildButton = Instantiate(_actionButtonPrefab, transform);
        _buildButton.SetupButton(type, _selectedUnits);
        _actionButtons.Add(_buildButton);
    }

    private void BuildButton(EButtonType type, List<Building> _selectedBuildings)
    {
        UI_Action_Button _buildButton = Instantiate(_actionButtonPrefab, transform);
        _buildButton.SetupButton(type, _selectedBuildings);
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

    private void BuildBlankButton(int amount = 1)
    {
        if (amount > 0)
        {
            List<Unit> _selectedUnits = new List<Unit>();
            for (int i = 0; i < amount; i++)
            {
                BuildButton(EButtonType.Blank, _selectedUnits);
            }
        }
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

    public void UpdateCanAffords()
    {
        if (_actionButtons.Count > 0)
        {
            foreach (UI_Action_Button action_Button in _actionButtons)
            {
                action_Button.UpdateCanAfford();
            }
        }
    }
}
// Writen by Lukasz Dziedziczak