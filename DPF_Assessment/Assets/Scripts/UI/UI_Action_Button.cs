// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Action_Button : MonoBehaviour
{
    private UnitProducer _unitProducer;
    private Unit.EUnitType _buildableUnit;
    private Faction _faction;
    private Building.EBuildingType _constructableBuilding;
    private Button _button;
    private List<BuildingConstructor> _constructionTeam;
    private List<Unit> _selectedUnits = new List<Unit>();
    private UI_Action.EButtonType _buttonType;

    private void Awake()
    {
        _button = GetComponentInChildren<Button>();
    }

    private void Start()
    {
        _button.onClick.AddListener(OnClick);
    }

    public void SetupButton(UnitProducer _newUnitProducer, Unit.EUnitType _newBuildableUnit, Texture _newIcon)
    {
        _unitProducer = _newUnitProducer;
        _buildableUnit = _newBuildableUnit;
        _button.GetComponent<RawImage>().texture = _newIcon;
    }

    private void OnClick()
    {
        if (_unitProducer != null)
        {
            _unitProducer.AddToQue(_buildableUnit);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                for(int i = 0; i < 4; i++)
                {
                    _unitProducer.AddToQue(_buildableUnit);
                }
            }
        }

        else if (_faction != null)
        {
            if (_faction.CanAfford(_constructableBuilding))
            {
                _faction.SubtractFromStockpileCostOf(_constructableBuilding);
                _faction.SpawnBuilding(_constructableBuilding, _constructionTeam);
            }
        }

        else if (_selectedUnits.Count > 0)
        {
            FindObjectOfType<GameController>().HUD_Manager().Actions_HUD().ActionButton(_buttonType, _selectedUnits);
        }

    }

    public void SetupButton(Faction _newFaction, Building.EBuildingType _buildingType, Texture _newIcon, List<BuildingConstructor> _newTeam)
    {
        _faction = _newFaction;
        _constructableBuilding = _buildingType;
        _button.GetComponent<RawImage>().texture = _newIcon;
        _constructionTeam = _newTeam;
    }

    public void SetupButton(UI_Action.EButtonType _newButtonType, List<Unit> _newSelection)
    {
        _buttonType = _newButtonType;
        _selectedUnits = _newSelection;
        _button.GetComponent<RawImage>().texture = Load_hudIcon(_buttonType);
    }

    private Texture Load_hudIcon(UI_Action.EButtonType _type)
    {
        switch (_type)
        {
            case UI_Action.EButtonType.Build:
                return (Texture)Resources.Load<Texture>("HUD_Icons/Hammer");

            case UI_Action.EButtonType.Back:
                return (Texture)Resources.Load<Texture>("HUD_Icons/Back");

            default:
                return null;
        }
    }
}
// Writen by Lukasz Dziedziczak