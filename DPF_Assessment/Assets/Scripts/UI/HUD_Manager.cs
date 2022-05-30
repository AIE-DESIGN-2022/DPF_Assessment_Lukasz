// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_Manager : MonoBehaviour
{
    private UI_Resources _resourcesUI;
    private UI_Action _actionsUI;
    private List<Selectable> _selection;
    private FactionConfig _config;
    private UI_Info _infoUI;

    private void Awake()
    {
        _resourcesUI = GetComponentInChildren<UI_Resources>();
        _actionsUI = GetComponentInChildren<UI_Action>();
        _infoUI = GetComponentInChildren<UI_Info>();
    }

    public void SetPlayerFaction(Faction _playerFaction)
    {
        if (_resourcesUI != null) _resourcesUI.SetPlayerFaction(_playerFaction);
        if (_actionsUI != null) _actionsUI.SetFaction(_playerFaction);
        _config = _playerFaction.Config();
        if (_infoUI != null) _infoUI.SetFactionConfig(_config);
    }
    
    public UI_Resources Resource_HUD()
    {
        if (_resourcesUI !=null) return _resourcesUI;
        else return null;
    }

    public UI_Info Info_HUD()
    {
        return _infoUI;
    }

    public UI_Action Actions_HUD()
    {
        return _actionsUI;
    }

    public void NewSelection(List<Selectable> _newSelection)
    {
        ClearSelection();
        _selection = _newSelection;
        //print("HUD manager sees new selection");

        if (_selection.Count == 1)
        {
            Unit _selectedUnit = _selection[0].GetComponent<Unit>();
            Building _selectedBuilding = _selection[0].GetComponent<Building>();
            CollectableResource _selectedCollectableResource = _selection[0].GetComponent<CollectableResource>();


            if (_selectedBuilding != null)
            {
                _infoUI.NewSelection(_selectedBuilding);
                _actionsUI.BuildingSelected(_selectedBuilding);
            }

            if (_selectedUnit != null)
            {
                _infoUI.NewSelection(_selectedUnit);
                _actionsUI.UnitSelected(_selectedUnit);
            }

            if (_selectedCollectableResource != null)
            {
                _infoUI.NewSelection(_selectedCollectableResource);
            }
        }

        if (_selection.Count > 1)
        {
            List<Unit> selectedUnits = new List<Unit>();
            List<Building> selectedBuildings = new List<Building>();
            List<CollectableResource> selectedResources = new List<CollectableResource> ();

            foreach (Selectable item in _selection)
            {
                Unit _selectedUnit = item.GetComponent<Unit>();
                Building _selectedBuilding = item.GetComponent<Building>();
                CollectableResource _selectedCollectableResource = item.GetComponent<CollectableResource>();

                if (_selectedUnit != null) selectedUnits.Add(_selectedUnit);
                if (_selectedBuilding != null) selectedBuildings.Add(_selectedBuilding);
                if (_selectedCollectableResource != null) selectedResources.Add(_selectedCollectableResource);
            }

            if (selectedUnits.Count > 0)
            {
                _actionsUI.UnitsSelected(selectedUnits);
            }
        }
        
    }

    public void ClearSelection()
    {
        _selection = null;
        _actionsUI.Clear();
        _infoUI.ClearSelection();
    }

    public void UpdateResources()
    {
        _resourcesUI.UpdateResources();
        _actionsUI.UpdateCanAffords();
    }
}
// Writen by Lukasz Dziedziczak