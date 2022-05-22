using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_Manager : MonoBehaviour
{
    private UI_Resources _resourcesUI;

    private void Awake()
    {
        _resourcesUI = GetComponentInChildren<UI_Resources>();
    }

    public void SetPlayerFaction(Faction _playerFaction)
    {
        if (_resourcesUI != null) _resourcesUI.SetPlayerFaction(_playerFaction);
    }
    
    public UI_Resources Resource_HUD()
    {
        if (_resourcesUI !=null) return _resourcesUI;
        else return null;
    }
}
