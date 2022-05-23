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
    }
    
}
