// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Info : MonoBehaviour
{
    private Canvas canvas;
    private FactionConfig config;
    private RawImage icon;
    private Text _name;
    private Text status1;
    private Text status2;
    private Unit selectedUnit;
    private Building selectedBuilding;
    private UI_Bar healthBar;
    private UI_Bar buildingBar;
    private RawImage buildingIcon;
    private Button buildingIconButton;
    private UnitProducer unitProducer;
    private UI_BuildQue buildQueUI;
    //private Building _buildingGettingConstructed; //NOT SURE IF BEING USED
    private BuildingConstructor buildingConstructor;
    private CollectableResource selectedResource;

    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();
        FindImageFields();
        FindTextFields();
        FindStatusBars();
        buildQueUI = GetComponentInChildren<UI_BuildQue>();
        buildingIconButton = GetComponentInChildren<Button>();
    }

    private void FindImageFields()
    {
        RawImage[] imageFields = GetComponentsInChildren<RawImage>();
        foreach (RawImage imageField in imageFields)
        {
            if (imageField.name == "Icon") icon = imageField;
            if (imageField.name == "BuildingIcon") buildingIcon = imageField;
        }

        if (icon == null) Debug.LogError(name + " could not find Icon image field.");
        if (buildingIcon == null) Debug.LogError(name + " could not find Building Icon image field.");

    }

    private void FindStatusBars()
    {
        UI_Bar[] bars = GetComponentsInChildren<UI_Bar>();
        foreach (UI_Bar bar in bars)
        {
            if (bar.name == "HealthBar") healthBar = bar;
            if (bar.name == "BuildingBar") buildingBar = bar;
        }
    }

    private void FindTextFields()
    {
        Text[] fields = GetComponentsInChildren<Text>();
        foreach (Text field in fields)
        {
            if (field.name == "Name") _name = field;
            if (field.name == "Status1") status1 = field;
            if (field.name == "Status2") status2 = field;
        }
    }

    private void Start()
    {
        buildingIconButton.onClick.AddListener(CancelBuildItem);
        ClearSelection();
    }

    private void CancelBuildItem()
    {
        if (unitProducer != null)
        {
            unitProducer.CancelBuildItem();

            if (Input.GetKey(KeyCode.LeftShift))
            {
                for (int i = 0; i < 4; i++)
                {
                    unitProducer.RemoveFromQue(unitProducer.CurrentlyProducing());
                }
            }
        }
    }

    public void SetFactionConfig(FactionConfig _newConfig)
    {
        config = _newConfig;
    }

    public void Show(bool isShowing)
    {
        if (canvas != null)
        {
            canvas.enabled = isShowing;
        }
    }

    public void NewSelection(Unit newSelectedUnit)
    {
        Show(true);
        selectedUnit = newSelectedUnit;
        _name.text = selectedUnit.name.Replace("(Clone)", "");
        icon.texture = selectedUnit.Faction().Config().Icon(selectedUnit.UnitType());
        UpdateStatus();

        buildingConstructor = selectedUnit.GetComponent<BuildingConstructor>();
        UpdateBuildingStatus();

        healthBar.Set(selectedUnit.GetComponent<Health>());
    }

    public void NewSelection(Building newSelectedBuilding)
    {
        Show(true);
        selectedBuilding = newSelectedBuilding;
        _name.text = selectedBuilding.name.Replace("(Clone)", "");
        icon.texture = selectedBuilding.Faction().Config().Icon(selectedBuilding.BuildingType());
        UpdateStatus();

        unitProducer = selectedBuilding.GetComponent<UnitProducer>();
        UpdateBuildingStatus();
        UpdateBuildQue();

        healthBar.Set(selectedBuilding.GetComponent<Health>());

    }

    public void NewSelection(CollectableResource newSelectedResource)
    {
        Show(true);
        selectedResource = newSelectedResource;
        _name.text = selectedResource.name;
        icon.texture = LoadResourceTexture(selectedResource);
        UpdateStatus();
    }

    private Texture LoadResourceTexture(CollectableResource newSelectedResource)
    {
        switch(newSelectedResource.ResourceType())
        {
            case CollectableResource.EResourceType.Wood:
                return (Texture)Resources.Load<Texture>("HUD_Icons/resource_tree");

            case CollectableResource.EResourceType.Food:
                if (newSelectedResource.GetComponent<Building>() == null)
                {
                    return (Texture)Resources.Load<Texture>("HUD_Icons/resource_fruitTree");
                }
                else return newSelectedResource.GetComponent<Building>().Faction().Config().Icon(selectedBuilding.BuildingType());

            case CollectableResource.EResourceType.Gold:
                return (Texture)Resources.Load<Texture>("HUD_Icons/resource_gold");

            default:
                return null;
        }
    }

    public void UpdateBuildingStatus()
    {
        if (unitProducer != null)
        {
            if (unitProducer.IsCurrentlyProducing())
            {
                if (!buildingIcon.enabled)
                {
                    buildingIcon.enabled = true;
                    buildingIcon.texture = config.Icon(unitProducer.CurrentlyProducing());
                    buildingBar.Set(unitProducer);
                }

            }
            else
            {
                if (buildingIcon.enabled)
                {
                    buildingIcon.enabled = false;
                    buildingIcon.texture = null;
                    buildingBar.Clear();
                }
            }
        }

        if (buildingConstructor != null)
        {
            if (buildingConstructor.HasBuildTarget())
            {
                buildingIcon.enabled = true;
                buildingIcon.texture = config.Icon(buildingConstructor.BuildTarget().BuildingType());
                buildingBar.Set(buildingConstructor.BuildTarget());
            }
            else
            {
                buildingIcon.enabled = false;
                buildingIcon.texture = null;
                buildingBar.Clear();
            }
        }
    }

    public void UpdateBuildQue()
    {
        if (unitProducer != null)
        {
            if (unitProducer.BuildQue().Count > 0)
            {
                buildQueUI.UpdateUIQue(unitProducer, config);
            }
            else
            {
                buildQueUI.Clear();
            }
        }
    }

    public void UpdateHealthBar()
    {
        healthBar.UpdateHealthBar();
    }

    public void ClearSelection()
    {
        Show(false);
        selectedBuilding = null;
        selectedUnit = null;
        _name.text = string.Empty;
        status1.text = string.Empty;
        status2.text = string.Empty;
        unitProducer = null;
        buildingIcon.enabled = false;
        buildingBar.Clear();
        buildQueUI.Clear();
        healthBar.Clear();
        //_buildingGettingConstructed = null;
        buildingConstructor = null;
        selectedResource = null;
        icon.texture = null;
    }

    public void UpdateStatus()
    {
        string _newStatus1 = "";
        string _newStatus2 = "";

        if (selectedUnit != null)
        {
            selectedUnit.Status(out _newStatus1, out _newStatus2);
        }

        if (selectedBuilding != null)
        {
            selectedBuilding.Status(out _newStatus1, out _newStatus2);
        }

        if (selectedResource != null)
        {
            selectedResource.Status(out _newStatus1, out _newStatus2);
        }

        status1.text = _newStatus1;
        status2.text = _newStatus2;
    }
}
// Writen by Lukasz Dziedziczak