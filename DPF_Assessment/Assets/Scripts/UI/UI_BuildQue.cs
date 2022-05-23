using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BuildQue : MonoBehaviour
{
    private List<Unit.EUnitType> _buildQue;
    private UI_BuildQueItem _buildQueItemPrefab;
    private UnitProducer _buildQueProducer;
    private FactionConfig _config;
    private List<UI_BuildQueItem> _queItemList = new List<UI_BuildQueItem>();

    private void Awake()
    {
        _buildQueItemPrefab = (UI_BuildQueItem)Resources.Load<UI_BuildQueItem>("HUD_Prefabs/UI_BuildQueItem");
    }

    public void UpdateUIQue(UnitProducer _newUnitProducer, FactionConfig _newConfig)
    {
        Clear();
        _buildQueProducer = _newUnitProducer;
        _buildQue = _newUnitProducer.BuildQue();
        _config = _newConfig;
        foreach (Unit.EUnitType _queItem in _buildQue)
        {
             UI_BuildQueItem _newItem = Instantiate(_buildQueItemPrefab, transform);
            _queItemList.Add(_newItem);
            _newItem.Set(_queItem, _buildQueProducer, _config.Icon(_queItem));
        }
    }

    public void Clear()
    {
        if (_queItemList.Count > 0)
        {
            foreach (UI_BuildQueItem _queItem in _queItemList)
            {
                Destroy(_queItem.gameObject);
            }
            _queItemList.Clear();
        }
    }
}
