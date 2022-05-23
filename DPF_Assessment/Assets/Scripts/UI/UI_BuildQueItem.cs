using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildQueItem : MonoBehaviour
{
    private RawImage _image;
    private Button _button;
    private UnitProducer _unitProducer;
    private Unit.EUnitType _unit;

    private void Awake()
    {
        _button = GetComponentInChildren<Button>();
        _image = GetComponentInChildren<RawImage>();
    }

    private void Start()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        _unitProducer.RemoveFromQue(_unit);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            for (int i = 0; i < 4; i++)
            {
                _unitProducer.RemoveFromQue(_unit);
            }
        }
    }

    public void Set(Unit.EUnitType _newUnit, UnitProducer _newUnitProducer, Texture _icon)
    {
        _unitProducer = _newUnitProducer;
        _unit = _newUnit;
        _image.texture = _icon;
    }
}
