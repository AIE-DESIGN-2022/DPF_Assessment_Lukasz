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
        foreach (Text textComponent in textComponents)
        {
            if (textComponent.name == "Food_Text") food = textComponent;
            if (textComponent.name == "Wood_Text") wood = textComponent;
            if (textComponent.name == "Gold_Text") gold = textComponent;
        }

        if (food == null) Debug.LogError(name + " could not find food textbox.");
        if (wood == null) Debug.LogError(name + " could not find wood textbox.");
        if (gold == null) Debug.LogError(name + " could not find Food textbox.");
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