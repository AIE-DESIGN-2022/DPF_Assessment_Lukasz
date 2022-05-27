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
    private Image background;
    private bool flashing = false;
    [SerializeField] private float flashingSpeed = 0.25f;
    private float flashingTimer;

    private void Awake()
    {
        _button = GetComponentInChildren<Button>();
        background = GetComponentInChildren<Image>();
    }

    private void Start()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        Flashing();
    }

    public void SetupButton(UnitProducer _newUnitProducer, Unit.EUnitType _newBuildableUnit, Texture _newIcon)
    {
        _unitProducer = _newUnitProducer;
        _buildableUnit = _newBuildableUnit;
        _button.GetComponent<RawImage>().texture = _newIcon;
        SetBackgroundActive(false);
    }

    public void SetupButton(Faction _newFaction, Building.EBuildingType _buildingType, Texture _newIcon, List<BuildingConstructor> _newTeam)
    {
        _faction = _newFaction;
        _constructableBuilding = _buildingType;
        _button.GetComponent<RawImage>().texture = _newIcon;
        _constructionTeam = _newTeam;
        SetBackgroundActive(false);
    }

    public void SetupButton(UI_Action.EButtonType _newButtonType, List<Unit> _newSelection)
    {
        _buttonType = _newButtonType;
        _selectedUnits = _newSelection;

        if (_newButtonType != UI_Action.EButtonType.Blank)
        {
            _button.GetComponent<RawImage>().texture = Load_hudIcon(_buttonType);
            SetActiveStance();
        }
        else
        {
            _button.GetComponent<RawImage>().color = Color.clear;
            SetBackgroundActive(false);
        }
        
    }

    public void UpdateUnitStance()
    {
        SetActiveStance();
    }

    private void SetActiveStance()
    {
        if (_buttonType == UI_Action.EButtonType.StancePassive)
        {
            if (CheckIfSelectedUnitsHaveStance(Unit.EUnitStance.Passive)) SetBackgroundActive(true);
            else SetBackgroundActive(false);
        }

        else if (_buttonType == UI_Action.EButtonType.StanceDefensive)
        {
            if (CheckIfSelectedUnitsHaveStance(Unit.EUnitStance.Defensive)) SetBackgroundActive(true);
            else SetBackgroundActive(false);
        }

        else if (_buttonType == UI_Action.EButtonType.StanceOffensive)
        {
            if (CheckIfSelectedUnitsHaveStance(Unit.EUnitStance.Offensive)) SetBackgroundActive(true);
            else SetBackgroundActive(false);
        }

        else if (_buttonType == UI_Action.EButtonType.StancePatrol)
        {
            if (CheckIfSelectedUnitsHaveStance(Unit.EUnitStance.Patrol)) SetBackgroundActive(true);
            else SetBackgroundActive(false);
        }

        else
        {
            SetBackgroundActive(false);
        }
    }

    private bool CheckIfSelectedUnitsHaveStance(Unit.EUnitStance unitStance)
    {
        bool haveFoundWithStance = false;
        if (_selectedUnits.Count > 0)
        {
            foreach (Unit unit in _selectedUnits)
            {
                //print(name + " checking " + unit.name + " with " + unit.UnitStance().ToString());
                if (unit.UnitStance() == unitStance) haveFoundWithStance = true;
            }
        }
        //if (haveFoundWithStance) print("have found unit with stance " + unitStance);
        return haveFoundWithStance;
    }

    private void SetBackgroundActive(bool isActive)
    {
        if (background != null)
        {
            background.gameObject.SetActive(isActive);
            //print(name + " setting background active " + isActive);
        }
        else Debug.Log(name + " Action Button Background no found");
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
            if (_faction.CanAfford(_constructableBuilding) && !_faction.CurrentlyPlacingBuilding())
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

    private Texture Load_hudIcon(UI_Action.EButtonType _type)
    {
        switch (_type)
        {
            case UI_Action.EButtonType.Build:
                return (Texture)Resources.Load<Texture>("HUD_Icons/Hammer");

            case UI_Action.EButtonType.Back:
                return (Texture)Resources.Load<Texture>("HUD_Icons/Back");

            case UI_Action.EButtonType.StancePassive:
                return (Texture)Resources.Load<Texture>("HUD_Icons/Passive");

            case UI_Action.EButtonType.StanceDefensive:
                return (Texture)Resources.Load<Texture>("HUD_Icons/Defensive");

            case UI_Action.EButtonType.StanceOffensive:
                return (Texture)Resources.Load<Texture>("HUD_Icons/Offensive");

            case UI_Action.EButtonType.StancePatrol:
                return (Texture)Resources.Load<Texture>("HUD_Icons/Patrol");

            default:
                return null;
        }
    }

    private void Flashing()
    {
        if (flashing)
        {
            flashingTimer += Time.deltaTime;

            if (flashingTimer > flashingSpeed)
            {
                if (background != null && background.gameObject.activeSelf)
                {
                    background.gameObject.SetActive(false);
                }
                else if (background != null && !background.gameObject.activeSelf)
                {
                    background.gameObject.SetActive(true);
                }

                flashingTimer = 0;
            }
        }
    }

    public void SetFLashing(bool isFlashing)
    {
        flashing = isFlashing;
    }
}
// Writen by Lukasz Dziedziczak