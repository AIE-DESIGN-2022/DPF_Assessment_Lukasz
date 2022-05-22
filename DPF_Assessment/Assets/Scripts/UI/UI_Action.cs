using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Action : MonoBehaviour
{
    private List<UI_Action_Button> _actionButtons = new List<UI_Action_Button>();
    private UI_Action_Button _actionButtonPrefab;
    private FactionConfig _config;

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

    public void SetFactionConfig(FactionConfig _newConfig)
    {
        _config = _newConfig;
    }

    public void BuildingSelected(Building _selectedBuilding)
    {
        UnitProducer _unitProducer = _selectedBuilding.GetComponent<UnitProducer>();
        if (_unitProducer != null)
        {
            //print("Action UI sees unitProducer");
            List<Unit.EUnitType> _buildableUnits = _unitProducer.GetListOfBuildableUnits();

            foreach (Unit.EUnitType _buildableUnit in _buildableUnits)
            {
                UI_Action_Button _newButton = Instantiate(_actionButtonPrefab, transform);
                _newButton.SetupButton(_unitProducer, _buildableUnit, _config.Icon(_buildableUnit));
                _actionButtons.Add(_newButton);
            }
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
}
