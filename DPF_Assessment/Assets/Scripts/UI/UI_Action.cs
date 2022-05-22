using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Action : MonoBehaviour
{
    List<UI_Action_Button> _actionButtons = new List<UI_Action_Button>();
    private UI_Action_Button _actionButtonPrefab;

    private void Awake()
    {
        _actionButtonPrefab = Resources.Load<UI_Action_Button>("UI/");
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
                print("Creating button for " + _buildableUnit.ToString());
                UI_Action_Button _newButton = Instantiate(_actionButtonPrefab, transform);
                _actionButtons.Add(_newButton);
                _newButton.SetupButton(_unitProducer, _buildableUnit);
            }
        }
    }
}
