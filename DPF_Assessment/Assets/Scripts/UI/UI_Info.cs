using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Info : MonoBehaviour
{
    private Canvas _canvas;
    private FactionConfig _config;
    private RawImage _icon;
    private Text _name;
    private Text _status1;
    private Text _status2;
    private Unit _selectedUnit;
    private Building _selectedBuilding;
    private UI_Bar _healthBar;
    private UI_Bar _buildingBar;
    private RawImage _buildingIcon;
    private Button _buildingIconButton;
    private UnitProducer _unitProducer;
    private UI_BuildQue _buildQueUI;

    private void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>();
        FindImageFields();
        FindTextFields();
        FindStatusBars();
        _buildQueUI = GetComponentInChildren<UI_BuildQue>();
        _buildingIconButton = GetComponentInChildren<Button>();
    }

    private void FindImageFields()
    {
        RawImage[] _imageFields = GetComponentsInChildren<RawImage>();
        foreach (RawImage _imageField in _imageFields)
        {
            if (_imageField.name == "Icon") _icon = _imageField;
            if (_imageField.name == "BuildingIcon") _buildingIcon = _imageField;
        }
        
    }

    private void FindStatusBars()
    {
        UI_Bar[] _bars = GetComponentsInChildren<UI_Bar>();
        foreach (UI_Bar _bar in _bars)
        {
            if (_bar.name == "HealthBar") _healthBar = _bar;
            if (_bar.name == "BuildingBar") _buildingBar = _bar;
        }
    }

    private void FindTextFields()
    {
        Text[] _fields = GetComponentsInChildren<Text>();
        foreach (Text _field in _fields)
        {
            if (_field.name == "Name") _name = _field;
            if (_field.name == "Status1") _status1 = _field;
            if (_field.name == "Status2") _status2 = _field;
        }
    }

    private void Start()
    {
        _buildingIconButton.onClick.AddListener(CancelBuildItem);
        ClearSelection();
    }

    private void CancelBuildItem()
    {
        if (_unitProducer != null)
        {
            _unitProducer.CancelBuildItem();

            if (Input.GetKey(KeyCode.LeftShift))
            {
                for (int i = 0; i < 4; i++)
                {
                    _unitProducer.RemoveFromQue(_unitProducer.CurrentlyProducing());
                }
            }
        }
    }

    public void SetFactionConfig(FactionConfig _newConfig)
    {
        _config = _newConfig;
    }

    public void Show(bool isShowing)
    {
        if (_canvas != null)
        {
            _canvas.enabled = isShowing;
        }
    }

    public void NewSelection(Unit _newSelectedUnit)
    {
        Show(true);
        _selectedUnit = _newSelectedUnit;
        _name.text = _selectedUnit.name.Replace("(Clone)", "");
        if (_config != null)
        {
            _icon.texture = _config.Icon(_selectedUnit.UnitType());
        }
        else Debug.LogError(name + " is missing FactionConfig.");
        UpdateStatus();

        _healthBar.Set(_selectedUnit.GetComponent<Health>());
    }

    public void NewSelection(Building _newSelectedBuilding)
    {
        Show(true);
        _selectedBuilding = _newSelectedBuilding;
        _name.text = _selectedBuilding.name.Replace("(Clone)", "");
        if (_config != null)
        {
            _icon.texture = _config.Icon(_selectedBuilding.BuildingType());
        }
        else Debug.LogError(name + " is missing FactionConfig.");
        UpdateStatus();

        _unitProducer = _selectedBuilding.GetComponent<UnitProducer>();
        UpdateBuildingStatus();
        UpdateBuildQue();

        _healthBar.Set(_selectedBuilding.GetComponent<Health>());

    }

    public void UpdateBuildingStatus()
    {
        if (_unitProducer != null)
        {
            if (_unitProducer.IsCurrentlyProducing())
            {
                if (!_buildingIcon.enabled)
                {
                    _buildingIcon.enabled = true;
                    _buildingIcon.texture = _config.Icon(_unitProducer.CurrentlyProducing());
                    _buildingBar.Set(_unitProducer);
                }

            }
            else
            {
                if (_buildingIcon.enabled)
                {
                    _buildingIcon.enabled = false;
                    _buildingIcon.texture = null;
                    _buildingBar.Clear();
                }
            }
        }
    }

    public void UpdateBuildQue()
    {
        if (_unitProducer != null)
        {
            if (_unitProducer.BuildQue().Count > 0)
            {
                _buildQueUI.UpdateUIQue(_unitProducer, _config);
            }
            else
            {
                _buildQueUI.Clear();
            }
        }
    }

    public void UpdateHealthBar()
    {
        _healthBar.UpdateHealthBar();
    }

    public void ClearSelection()
    {
        Show(false);
        _selectedBuilding = null;
        _selectedUnit = null;
        _name.text = string.Empty;
        _status1.text = string.Empty;
        _status2.text = string.Empty;
        _unitProducer = null;
        _buildingIcon.enabled = false;
        _buildingBar.Clear();
        _buildQueUI.Clear();
        _healthBar.Clear();
    }

    public void UpdateStatus()
    {
        string _newStatus1 = "";
        string _newStatus2 = "";

        if (_selectedUnit != null)
        {
            _selectedUnit.Status(out _newStatus1, out _newStatus2);
        }

        if (_selectedBuilding != null)
        {
            _selectedBuilding.Status(out _newStatus1, out _newStatus2);
        }

        _status1.text = _newStatus1;
        _status2.text = _newStatus2;
    }
}
