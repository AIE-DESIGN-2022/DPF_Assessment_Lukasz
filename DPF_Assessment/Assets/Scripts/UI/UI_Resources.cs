// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Resources : MonoBehaviour
{
    private Faction faction;

    private Text food;
    private Text wood;
    private Text gold;

    private void Awake()
    {
        SetupTextBoxes();
    }

    private void SetupTextBoxes()
    {
        Text[] textComponents = GetComponentsInChildren<Text>();
        foreach (Text text in textComponents)
        {
            if (text.name == "Foodtext") food = text;
            if (text.name == "Woodtext") wood = text;
            if (text.name == "Goldtext") gold = text;
        }
    }

    public void SetPlayerFaction(Faction _playerFaction)
    {
        faction = _playerFaction;
        UpdateResources();
    }

    public void UpdateResources()
    {
        if (faction != null && food != null && wood != null && gold != null)
        {
            food.text = faction.StockpileAmount(CollectableResource.EResourceType.Food).ToString();
            wood.text = faction.StockpileAmount(CollectableResource.EResourceType.Wood).ToString();
            gold.text = faction.StockpileAmount(CollectableResource.EResourceType.Gold).ToString();
        }
    }
}
// Writen by Lukasz Dziedziczak