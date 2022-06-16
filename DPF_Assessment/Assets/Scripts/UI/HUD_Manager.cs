// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_Manager : MonoBehaviour
{
    private UI_Resources resourcesUI;
    private UI_Action actionsUI;
    private List<Selectable> selection;
    private FactionConfig config;
    private UI_Info infoUI;
    private UI_InfoMulti infoMultiUI;
    private UI_Tooltip tooltipUI;
    private UI_Menu pauseMenu;


    private void Awake()
    {
        resourcesUI = GetComponentInChildren<UI_Resources>();
        actionsUI = GetComponentInChildren<UI_Action>();
        infoUI = GetComponentInChildren<UI_Info>();
        infoMultiUI = GetComponentInChildren<UI_InfoMulti>();
        tooltipUI = GetComponentInChildren<UI_Tooltip>();
    }

    public void SetPlayerFaction(Faction playerFaction)
    {
        if (resourcesUI == null) Debug.LogError(name + " cannot find Resource HUD element.");

        if (resourcesUI != null) resourcesUI.SetPlayerFaction(playerFaction);
        if (actionsUI != null) actionsUI.SetFaction(playerFaction);
        config = playerFaction.Config();
        if (infoUI != null) infoUI.SetFactionConfig(config);
    }
    
    public UI_Resources Resource_HUD()
    {
        if (resourcesUI !=null) return resourcesUI;
        else return null;
    }

    public UI_Info Info_HUD()
    {
        return infoUI;
    }

    public UI_InfoMulti InfoMulti_HUD()
    {
        return infoMultiUI;
    }

    public UI_Action Actions_HUD()
    {
        return actionsUI;
    }

    public void NewSelection(List<Selectable> newSelection)
    {
        ClearSelection();
        selection = newSelection;
        //print("HUD manager sees new selection");

        if (selection.Count == 1)
        {
            Unit selectedUnit = selection[0].GetComponent<Unit>();
            Building selectedBuilding = selection[0].GetComponent<Building>();
            CollectableResource selectedCollectableResource = selection[0].GetComponent<CollectableResource>();


            if (selectedBuilding != null)
            {
                infoUI.NewSelection(selectedBuilding);
                actionsUI.BuildingSelected(selectedBuilding);
            }

            if (selectedUnit != null)
            {
                infoUI.NewSelection(selectedUnit);
                actionsUI.UnitSelected(selectedUnit);
            }

            if (selectedCollectableResource != null)
            {
                infoUI.NewSelection(selectedCollectableResource);
            }
        }

        if (selection.Count > 1)
        {
            List<Unit> selectedUnits = new List<Unit>();
            List<Building> selectedBuildings = new List<Building>();
            List<CollectableResource> selectedResources = new List<CollectableResource> ();

            foreach (Selectable item in selection)
            {
                Unit selectedUnit = item.GetComponent<Unit>();
                Building selectedBuilding = item.GetComponent<Building>();
                CollectableResource selectedCollectableResource = item.GetComponent<CollectableResource>();

                if (selectedUnit != null) selectedUnits.Add(selectedUnit);
                if (selectedBuilding != null) selectedBuildings.Add(selectedBuilding);
                if (selectedCollectableResource != null) selectedResources.Add(selectedCollectableResource);
            }

            if (selectedUnits.Count > 0)
            {
                actionsUI.UnitsSelected(selectedUnits);
            }

            infoMultiUI.NewSelection(selection);
        }
        
    }

    public void ClearSelection()
    {
        selection = null;
        actionsUI.Clear();
        infoUI.ClearSelection();
        infoMultiUI.ClearSelection();
    }

    public void UpdateResources()
    {
        resourcesUI.UpdateResources();
        actionsUI.UpdateCanAffords();
    }

    public UI_Tooltip Tooltip_HUD()
    {
        return tooltipUI;
    }

    public void IdleWorkerButton()
    {
        GameController gameController = FindObjectOfType<GameController>();
        List<Selectable> idleWorkers = gameController.GetPlayerFaction().IdleWorkers();
        gameController.PlayerController().ClearSelection();
        gameController.PlayerController().AddToSelection(idleWorkers);

    }
}
// Writen by Lukasz Dziedziczak