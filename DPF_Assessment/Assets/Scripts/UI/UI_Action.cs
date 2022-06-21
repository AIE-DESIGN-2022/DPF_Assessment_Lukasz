// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Action : MonoBehaviour
{
    private List<UI_Action_Button> actionButtons = new List<UI_Action_Button>();
    private UI_Action_Button actionButtonPrefab;
    private Faction faction;

    public enum EButtonType
    {
        Build,
        Back,
        Blank,
        StancePassive,
        StanceDefensive,
        StanceOffensive,
        StancePatrol,
        AttackMove
    }

    private void Awake()
    {
        actionButtonPrefab = (UI_Action_Button)Resources.Load<UI_Action_Button>("HUD_Prefabs/UI_Action_Button");
    }

    private void Start()
    {
        if (actionButtonPrefab == null)
        {
            Debug.LogError(name + " is missing ActionButtonPrefab.");
            return;
        }
    }

    public void SetFaction(Faction newFaction)
    {
        faction = newFaction;
    }

    public void BuildingSelected(Building selectedBuilding)
    {
        if (faction.PlayerNumber() != selectedBuilding.PlayerNumber()) return;

        List<Building> selectedBuildings = new List<Building>();
        selectedBuildings.Add(selectedBuilding);
        BuildingsSelected(selectedBuildings);
    }

    public void BuildingsSelected(List<Building> selectedBuildings)
    {
        UnitProducer unitProducer = selectedBuildings[0].GetComponent<UnitProducer>();
        if (unitProducer != null && selectedBuildings[0].BuildState() == Building.EBuildState.Complete)
        {
            //print("Action UI sees unitProducer");
            List<Unit.EUnitType> buildableUnits = unitProducer.GetListOfBuildableUnits();

            foreach (Unit.EUnitType buildableUnit in buildableUnits)
            {
                BuildButton(buildableUnit, unitProducer);
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

    public void UnitSelected(Unit selectedUnit)
    {
        if (faction.PlayerNumber() != selectedUnit.PlayerNumber()) return;

        List<Unit> newTeam = new List<Unit>();
        newTeam.Add(selectedUnit);
        UnitsSelected(newTeam);
    }

    public void UnitsSelected(List<Unit> selectedUnits)
    {
        Clear();

        // 1st button
        if (AnyAreBuildingConstructors(selectedUnits))
        {
            BuildButton(EButtonType.Build, selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, selectedUnits);
        }

        // 2nd button
        if (AnyAreAttackers(selectedUnits))
        {
            BuildButton(EButtonType.AttackMove, selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, selectedUnits);
        }
        

        // 3rd button
        if (AnyAreAttackers(selectedUnits))
        {
            BuildButton(EButtonType.StancePatrol, selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, selectedUnits);
        }

        // 4th button
        if (AnyAreAttackers(selectedUnits) || AnyAreHealers(selectedUnits))
        {
            BuildButton(EButtonType.StancePassive, selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, selectedUnits);
        }

        // 5th button
        if (AnyAreAttackers(selectedUnits) || AnyAreHealers(selectedUnits))
        {
            BuildButton(EButtonType.StanceDefensive, selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, selectedUnits);
        }

        // 6th button
        if (AnyAreAttackers(selectedUnits))
        {
            BuildButton(EButtonType.StanceOffensive, selectedUnits);
        }
        else
        {
            BuildButton(EButtonType.Blank, selectedUnits);
        }
    }

    public void BuildButtonPushed(List<Unit> selectedUnits)
    {
        Clear();
        List<BuildingConstructor> constructionTeam = new List<BuildingConstructor>();
        foreach (Unit unit in selectedUnits)
        {
            BuildingConstructor constructor = unit.GetComponent<BuildingConstructor>();
            if (constructor) constructionTeam.Add(constructor);
        }

        List<Building.EBuildingType> constructableBuildings = faction.Config().ConstructableBuildings(selectedUnits[0].UnitType());

        BuildButton(EButtonType.Back, selectedUnits);

        //make buttons for each building.
        foreach (Building.EBuildingType buildingType in constructableBuildings)
        {
            BuildButton(buildingType, constructionTeam);
        }
    }

    public void Clear()
    {
        if (actionButtons.Count > 0)
        {
            foreach (UI_Action_Button button in actionButtons)
            {
                Destroy(button.gameObject);
            }
            actionButtons.Clear();
        }
    }

    public void ActionButton(EButtonType pushedButton, List<Unit> units)
    {
        switch (pushedButton)
        {
            case EButtonType.Build:
                BuildButtonPushed(units);
                break;

            case EButtonType.Back:
                UnitsSelected(units);
                break;

            case EButtonType.StancePassive:
                ChangeStanceOnUnits(units, Unit.EUnitStance.Passive);
                break;

            case EButtonType.StanceDefensive:
                ChangeStanceOnUnits(units, Unit.EUnitStance.Defensive);
                break;

            case EButtonType.StanceOffensive:
                ChangeStanceOnUnits(units, Unit.EUnitStance.Offensive);
                break;

            case EButtonType.StancePatrol:
                ChangeStanceOnUnits(units, Unit.EUnitStance.Patrol);
                break;

            case EButtonType.AttackMove:
                AttackMoveButtonPushed(units);
                break;

        }
    }

    public void ActionButton(EButtonType pushedButton, List<Building> buildings)
    {
        switch (pushedButton)
        {
            /*case EButtonType.Build:
                BuildButtonPushed(units);
                break;

            case EButtonType.Back:
                UnitsSelected(units);
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

    private bool AnyAreBuildingConstructors(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            if (unit.GetComponent<BuildingConstructor>() != null) return true;
        }
        return false;
    }

    private bool AnyAreAttackers(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            if (unit.GetComponent<Attacker>() != null) return true;
        }
        return false;
    }

    private bool AnyAreHealers(List<Unit> selectedUnits)
    {
        foreach (Unit unit in selectedUnits)
        {
            if (unit.GetComponent<Healer>() != null) return true;
        }
        return false;
    }

    private void ChangeStanceOnUnits(List<Unit> units, Unit.EUnitStance newStance)
    {
        foreach (Unit unit in units)
        {
            Healer healer = unit.GetComponent<Healer>();

            if (healer != null && (newStance == Unit.EUnitStance.Offensive || newStance == Unit.EUnitStance.Patrol))
            {
                unit.ChangeUnitStance(Unit.EUnitStance.Defensive);
            }
            else
            {
                unit.ChangeUnitStance(newStance);
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

    private void BuildButton(EButtonType type, List<Unit> selectedUnits)
    {
        UI_Action_Button buildButton = Instantiate(actionButtonPrefab, transform);
        buildButton.SetupButton(type, selectedUnits);
        actionButtons.Add(buildButton);
    }

    private void BuildButton(EButtonType type, List<Building> selectedBuildings)
    {
        UI_Action_Button buildButton = Instantiate(actionButtonPrefab, transform);
        buildButton.SetupButton(type, selectedBuildings);
        actionButtons.Add(buildButton);
    }

    private void BuildButton(Unit.EUnitType unitType, UnitProducer unitProducer)
    {
        UI_Action_Button newButton = Instantiate(actionButtonPrefab, transform);
        newButton.SetupButton(unitProducer, unitType, faction.Config().Icon(unitType));
        actionButtons.Add(newButton);
    }

    private void BuildButton(Building.EBuildingType buildingType, List<BuildingConstructor> constructionTeam)
    {
        UI_Action_Button newButton = Instantiate(actionButtonPrefab, transform);
        newButton.SetupButton(faction, buildingType, faction.Config().Icon(buildingType), constructionTeam);
        actionButtons.Add(newButton);
    }

    private void BuildBlankButton(int amount = 1)
    {
        if (amount > 0)
        {
            List<Unit> selectedUnits = new List<Unit>();
            for (int i = 0; i < amount; i++)
            {
                BuildButton(EButtonType.Blank, selectedUnits);
            }
        }
    }

    public void UpdateUnitStances()
    {
        if (actionButtons.Count > 0)
        {
            foreach (UI_Action_Button actionButton in actionButtons)
            {
                actionButton.UpdateUnitStance();
            }
        }
    }

    public void UpdateCanAffords()
    {
        if (actionButtons.Count > 0)
        {
            foreach (UI_Action_Button actionButton in actionButtons)
            {
                actionButton.UpdateCanAfford();
            }
        }
    }

    public void AttackMoveButtonPushed(List<Unit> selectedUnits)
    {
        if (selectedUnits.Count > 0)
        {
            foreach (Unit unit in selectedUnits)
            {
                unit.StartAttackMove();
            }
        }
    }
}
// Writen by Lukasz Dziedziczak