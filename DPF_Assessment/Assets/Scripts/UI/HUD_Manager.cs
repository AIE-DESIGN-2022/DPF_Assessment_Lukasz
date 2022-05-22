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

    private void Awake()
    {
        _resourcesUI = GetComponentInChildren<UI_Resources>();
        _actionsUI = GetComponentInChildren<UI_Action>();
    }

    public void SetPlayerFaction(Faction _playerFaction)
    {
        if (_resourcesUI != null) _resourcesUI.SetPlayerFaction(_playerFaction);
        _config = _playerFaction.Config();
        if (_actionsUI != null) _actionsUI.SetFactionConfig(_config);
    }
    
    public UI_Resources Resource_HUD()
    {
        if (_resourcesUI !=null) return _resourcesUI;
        else return null;
    }

    public void NewSelection(List<Selectable> _newSelection)
    {
        _selection = _newSelection;
        //print("HUD manager sees new selection");

        if (_selection[0].GetComponent<Building>() != null)
        {
            _actionsUI.BuildingSelected(_selection[0].GetComponent<Building>());
        }
    }

    public void ClearSelection()
    {
        _selection = null;
        _actionsUI.Clear();
    }
}
