using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Bar : MonoBehaviour
{
    private RectTransform _foreground;
    private RectTransform _background;
    private UnitProducer _unitProducer;
    private float _updateFreqency = 0.5f;
    private float _updateTime = 0;
    private Health _health;

    private void Awake()
    {
        FindRectTransforms();
    }

    private void FindRectTransforms()
    {
        RectTransform[] _rectTrans = GetComponentsInChildren<RectTransform>();
        foreach (RectTransform _rectTran in _rectTrans)
        {
            if (_rectTran.name == "Foreground") _foreground = _rectTran;
            if (_rectTran.name == "Background") _background = _rectTran;
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
    }

    private void ProducingUnit()
    {
        if (_unitProducer != null)
        {
            _updateTime += Time.deltaTime;
            if (_updateTime > _updateFreqency && _unitProducer.IsCurrentlyProducing())
            {
                UpdatePercentage(_unitProducer.PercentageComplete());
                _updateTime = 0;
            }
        }

    }

    public void Set(UnitProducer _producer)
    {
        _unitProducer = _producer;
        _updateTime = Mathf.Infinity;
    }

    public void Set(Health _newHealth)
    {
        _health = _newHealth;
        ShowBackground();
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        if (_health != null)
        {
            UpdatePercentage(_health.HealthPercentage());
        }
    }



    public void Clear()
    {
        _unitProducer = null;
        _health = null;
        UpdatePercentage(0);
        ShowBackground(false);
    }

    public void UpdatePercentage(float _percentage)
    {
        if (_foreground != null)
        {
            _foreground.localScale = new Vector3(_percentage, 1.0f, 1.0f);
        }
    }

    private void ShowBackground(bool show = true)
    {
        if (_background != null)
        {
            if (show)
            {
                _background.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else
            {
                _background.localScale = new Vector3(0.0f, 1.0f, 1.0f);
            }
        }
    }

}
