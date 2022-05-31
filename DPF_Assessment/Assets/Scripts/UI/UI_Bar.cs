// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Bar : MonoBehaviour
{
    private RectTransform foreground;
    private RectTransform background;
    private UnitProducer unitProducer;
    private float updateFreqency = 0.5f;
    private float updateTime = 0;
    private Health health;
    private Building construction;

    private void Awake()
    {
        FindRectTransforms();
    }

    private void FindRectTransforms()
    {
        RectTransform[] rectTrans = GetComponentsInChildren<RectTransform>();
        foreach (RectTransform rectTran in rectTrans)
        {
            if (rectTran.name == "Foreground") foreground = rectTran;
            if (rectTran.name == "Background") background = rectTran;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Clear();
    }

    // Update is called once per frame
    void Update()
    {
        ProducingUnit();
        ConstructingBuilding();
    }

    private void ConstructingBuilding()
    {
        if (construction != null)
        {
            updateTime += Time.deltaTime;
            if (updateTime > updateFreqency && construction.BuildState() == Building.EBuildState.Building)
            {
                UpdatePercentage(construction.PercentageComplete());
                updateTime = 0;
            }
        }    
    }

    private void ProducingUnit()
    {
        if (unitProducer != null)
        {
            updateTime += Time.deltaTime;
            if (updateTime > updateFreqency && unitProducer.IsCurrentlyProducing())
            {
                UpdatePercentage(unitProducer.PercentageComplete());
                updateTime = 0;
            }
        }
    }

    public void Set(UnitProducer producer)
    {
        unitProducer = producer;
        updateTime = Mathf.Infinity;
    }

    public void Set(Health newHealth)
    {
        health = newHealth;
        ShowBackground();
        UpdateHealthBar();
    }

    public void Set(Building newConstruction)
    {
        construction = newConstruction;
        updateTime = Mathf.Infinity;
    }

    public void UpdateHealthBar()
    {
        if (health != null)
        {
            UpdatePercentage(health.HealthPercentage());
        }
    }



    public void Clear()
    {
        unitProducer = null;
        health = null;
        construction = null;
        UpdatePercentage(0);
        ShowBackground(false);
    }

    public void UpdatePercentage(float percentage)
    {
        if (foreground != null)
        {
            foreground.localScale = new Vector3(percentage, 1.0f, 1.0f);
        }
    }

    private void ShowBackground(bool show = true)
    {
        if (background != null)
        {
            if (show)
            {
                background.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else
            {
                background.localScale = new Vector3(0.0f, 1.0f, 1.0f);
            }
        }
    }

}
// Writen by Lukasz Dziedziczak