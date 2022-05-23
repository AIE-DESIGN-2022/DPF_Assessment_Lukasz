// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Resources : MonoBehaviour
{
    private Faction _faction;

    private Text _food;
    private Text _wood;
    private Text _gold;

    private void Start()
    {
        SetupTextBoxes();
    }

    private void SetupTextBoxes()
    {
        Text[] _textComponents = GetComponentsInChildren<Text>();
        foreach (Text _text in _textComponents)
        {
            if (_text.name == "Food_Text") _food = _text;
            if (_text.name == "Wood_Text") _wood = _text;
            if (_text.name == "Gold_Text") _gold = _text;
        }
    }

    public void SetPlayerFaction(Faction _playerFaction)
    {
        _faction = _playerFaction;
        UpdateResources();
    }

    public void UpdateResources()
    {
        if (_faction != null && _food != null && _wood != null && _gold != null)
        {
            _food.text = _faction.StockpileAmount(CollectableResource.EResourceType.Food).ToString();
            _wood.text = _faction.StockpileAmount(CollectableResource.EResourceType.Wood).ToString();
            _gold.text = _faction.StockpileAmount(CollectableResource.EResourceType.Gold).ToString();
        }
    }
}
// Writen by Lukasz Dziedziczak