using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Action_Button : MonoBehaviour
{
    private UnitProducer _unitProducer;
    private Unit.EUnitType _buildableUnit;
    private Button _button;

    private void Awake()
    {
        _button = GetComponentInChildren<Button>();
    }

    private void Start()
    {
        _button.onClick.AddListener(OnClick);
    }

    public void SetupButton(UnitProducer _newUnitProducer, Unit.EUnitType _newBuildableUnit)
    {
        _unitProducer = _newUnitProducer;
        _buildableUnit = _newBuildableUnit;
    }

    private void OnClick()
    {
        if (_unitProducer != null) _unitProducer.AddToQue(_buildableUnit);
    }
    
}
