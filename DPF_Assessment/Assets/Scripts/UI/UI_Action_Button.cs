// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Action_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UnitProducer unitProducer;
    private Unit.EUnitType buildableUnit;
    private Faction faction;
    private Building.EBuildingType constructableBuilding;
    private Button button;
    private List<BuildingConstructor> constructionTeam;
    private List<Unit> selectedUnits = new List<Unit>();
    private List<Building> selectedBuildings = new List<Building> ();
    private UI_Action.EButtonType buttonType;
    private Image background;
    private bool flashing = false;
    [SerializeField] private float flashingSpeed = 0.25f;
    private float flashingTimer;
    private UI_Tooltip tooltipUI;
    private FactionConfig config;

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        background = GetComponentInChildren<Image>();
        
    }

    private void Start()
    {
        button.onClick.AddListener(OnClick);
        tooltipUI = FindObjectOfType<GameController>().HUD_Manager().Tooltip_HUD();
        config = FindObjectOfType<GameController>().GetPlayerFaction().Config();
    }

    private void Update()
    {
        Flashing();
    }

    public void SetupButton(UnitProducer newUnitProducer, Unit.EUnitType newBuildableUnit, Texture newIcon)
    {
        unitProducer = newUnitProducer;
        buildableUnit = newBuildableUnit;
        button.GetComponent<RawImage>().texture = newIcon;
        SetBackgroundActive(false);
        UpdateCanAfford();
    }

    public void SetupButton(Faction newFaction, Building.EBuildingType buildingType, Texture newIcon, List<BuildingConstructor> newTeam)
    {
        faction = newFaction;
        constructableBuilding = buildingType;
        button.GetComponent<RawImage>().texture = newIcon;
        constructionTeam = newTeam;
        SetBackgroundActive(false);
        UpdateCanAfford();
    }

    public void SetupButton(UI_Action.EButtonType newButtonType, List<Unit> newSelection)
    {
        buttonType = newButtonType;
        selectedUnits = newSelection;

        if (newButtonType != UI_Action.EButtonType.Blank)
        {
            button.GetComponent<RawImage>().texture = LoadhudIcon(buttonType);
            SetActiveStance();
        }
        else
        {
            button.GetComponent<RawImage>().color = Color.clear;
            SetBackgroundActive(false);
        }
        
    }

    public void SetupButton(UI_Action.EButtonType newButtonType, List<Building> newSelection)
    {
        buttonType = newButtonType;
        selectedBuildings = newSelection;

        if (newButtonType != UI_Action.EButtonType.Blank)
        {
            button.GetComponent<RawImage>().texture = LoadhudIcon(buttonType);
            SetActiveStance();
        }
        else
        {
            button.GetComponent<RawImage>().color = Color.clear;
            SetBackgroundActive(false);
        }

    }

    public void UpdateUnitStance()
    {
        SetActiveStance();
    }

    private void SetActiveStance()
    {
        if (buttonType == UI_Action.EButtonType.StancePassive)
        {
            if (CheckIfSelectedUnitsHaveStance(Unit.EUnitStance.Passive)) SetBackgroundActive(true);
            else SetBackgroundActive(false);
        }

        else if (buttonType == UI_Action.EButtonType.StanceDefensive)
        {
            if (CheckIfSelectedUnitsHaveStance(Unit.EUnitStance.Defensive)) SetBackgroundActive(true);
            else SetBackgroundActive(false);
        }

        else if (buttonType == UI_Action.EButtonType.StanceOffensive)
        {
            if (CheckIfSelectedUnitsHaveStance(Unit.EUnitStance.Offensive)) SetBackgroundActive(true);
            else SetBackgroundActive(false);
        }

        else if (buttonType == UI_Action.EButtonType.StancePatrol)
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
        if (selectedUnits.Count > 0)
        {
            foreach (Unit unit in selectedUnits)
            {
                if (unit.UnitStance() == unitStance) haveFoundWithStance = true;
            }
        }

        else if (selectedBuildings.Count > 0)
        {
            foreach (Building building in selectedBuildings)
            {
                Attacker tower = building.GetComponent<Attacker>();
                if (tower != null && tower.TowerStance() == unitStance) haveFoundWithStance=true;
            }
        }

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
        if (unitProducer != null)
        {
            unitProducer.AddToQue(buildableUnit);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                for(int i = 0; i < 4; i++)
                {
                    unitProducer.AddToQue(buildableUnit);
                }
            }
        }

        else if (faction != null)
        {
            if (faction.CanAfford(constructableBuilding) && !faction.CurrentlyPlacingBuilding())
            {
                faction.SubtractFromStockpileCostOf(constructableBuilding);
                faction.SpawnBuilding(constructableBuilding, constructionTeam);
            }
        }

        else if (selectedUnits.Count > 0)
        {
            FindObjectOfType<GameController>().HUD_Manager().Actions_HUD().ActionButton(buttonType, selectedUnits);
        }

        else if (selectedBuildings.Count > 0)
        {
            FindObjectOfType<GameController>().HUD_Manager().Actions_HUD().ActionButton(buttonType, selectedBuildings);
        }

    }

    private Texture LoadhudIcon(UI_Action.EButtonType type)
    {
        switch (type)
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

            case UI_Action.EButtonType.AttackMove:
                return (Texture)Resources.Load<Texture>("HUD_Icons/AttackMove");

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

    public void UpdateCanAfford()
    {
        if (unitProducer != null)
        {
            Faction buildingFaction = unitProducer.GetComponent<Building>().Faction;
            if (buildingFaction.CanAfford(buildableUnit))
            {
                button.interactable = true;
            }
            else
            {
                button.interactable = false;
            }
        }

        else if (constructionTeam != null)
        {
            if (faction.CanAfford(constructableBuilding))
            {
                button.interactable = true;
            }
            else
            {
                button.interactable = false;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipUI != null)
        {
            string topText = "";
            string bottomText = "";

            if (unitProducer != null)
            {
                topText = "Build " + config.PrefabName(buildableUnit);

                //bottomText = "Cost: ";
                int foodCost = config.UnitConfig(buildableUnit).foodCost;
                int woodCost = config.UnitConfig(buildableUnit).woodCost;
                int goldCost = config.UnitConfig(buildableUnit).goldCost;


                if (foodCost > 0)
                {
                    bottomText += foodCost + " Food ";
                }

                if (woodCost > 0)
                {
                    bottomText += woodCost + " Wood ";
                }

                if (goldCost > 0)
                {
                    bottomText += goldCost + " Gold";
                }
            }

            else if (faction != null)
            {
                topText = "Build " + config.PrefabName(constructableBuilding);

                int foodCost = config.BuildingConfig(constructableBuilding).foodCost;
                int woodCost = config.BuildingConfig(constructableBuilding).woodCost;
                int goldCost = config.BuildingConfig(constructableBuilding).goldCost;


                if (foodCost > 0)
                {
                    bottomText += foodCost + " Food ";
                }

                if (woodCost > 0)
                {
                    bottomText += woodCost + " Wood ";
                }

                if (goldCost > 0)
                {
                    bottomText += goldCost + " Gold";
                }
            }

            else if (selectedUnits.Count > 0 || selectedBuildings.Count > 0)
            {
                if (buttonType == UI_Action.EButtonType.Build)
                {
                    topText = "Build Menu";
                    bottomText = "Lists all buildings.";
                }
                else if (buttonType == UI_Action.EButtonType.Back)
                {
                    topText = "Go Back";
                    bottomText = "Back to previous menu.";
                }
                else if (buttonType == UI_Action.EButtonType.StancePassive)
                {
                    topText = "Passive Stance";
                    bottomText = "Will do nothing unless ordered.";
                }
                else if (buttonType == UI_Action.EButtonType.StanceDefensive)
                {
                    topText = "Defensive Stance";
                    bottomText = "Will attack if attacked.";
                }
                else if (buttonType == UI_Action.EButtonType.StanceOffensive)
                {
                    topText = "Offensive Stance";
                    bottomText = "Will attack on sight.";
                }
                else if (buttonType == UI_Action.EButtonType.StancePatrol)
                {
                    topText = "Patrol";
                    bottomText = "Walk between two points.";
                }

                else if (buttonType == UI_Action.EButtonType.AttackMove)
                {
                    topText = "Attack Move";
                    bottomText = "Move to a new location with aggresive stance.";
                }
            }

            tooltipUI.Show();
            tooltipUI.SetText(topText, bottomText);
        }
        else Debug.LogError(name + " has no ref to ToolTip_UI");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipUI != null) tooltipUI.Show(false);
    }
}
// Writen by Lukasz Dziedziczak