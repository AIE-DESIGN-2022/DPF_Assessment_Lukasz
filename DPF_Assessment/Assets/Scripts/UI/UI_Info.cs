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

    private void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>();
        _icon = GetComponentInChildren<RawImage>();
        FindTextFields();
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
        ClearSelection();
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
    }

    public void ClearSelection()
    {
        Show(false);
        _selectedBuilding = null;
        _selectedUnit = null;
        _name.text = string.Empty;
        _status1.text = string.Empty;
        _status2.text = string.Empty;
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
