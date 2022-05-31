// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildQueItem : MonoBehaviour
{
    private RawImage _image;
    private Button _button;
    private UnitProducer unitProducer;
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
        unitProducer.RemoveFromQue(_unit);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            for (int i = 0; i < 4; i++)
            {
                unitProducer.RemoveFromQue(_unit);
            }
        }
    }

    public void Set(Unit.EUnitType _newUnit, UnitProducer _newUnitProducer, Texture icon)
    {
        unitProducer = _newUnitProducer;
        _unit = _newUnit;
        _image.texture = icon;
    }
}
// Writen by Lukasz Dziedziczak