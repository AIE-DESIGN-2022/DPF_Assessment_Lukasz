// Writen by Lukasz Dziedziczak
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BuildQue : MonoBehaviour
{
    private List<Unit.EUnitType> buildQue;
    private UI_BuildQueItem buildQueItemPrefab;
    private UnitProducer buildQueProducer;
    private FactionConfig config;
    private List<UI_BuildQueItem> queItemList = new List<UI_BuildQueItem>();

    private void Awake()
    {
        buildQueItemPrefab = (UI_BuildQueItem)Resources.Load<UI_BuildQueItem>("HUD_Prefabs/UI_BuildQueItem");
    }

    public void UpdateUIQue(UnitProducer _newUnitProducer, FactionConfig _newConfig)
    {
        Clear();
        buildQueProducer = _newUnitProducer;
        buildQue = _newUnitProducer.BuildQue();
        config = _newConfig;
        foreach (Unit.EUnitType _queItem in buildQue)
        {
             UI_BuildQueItem _newItem = Instantiate(buildQueItemPrefab, transform);
            queItemList.Add(_newItem);
            _newItem.Set(_queItem, buildQueProducer, config.Icon(_queItem));
        }
    }

    public void Clear()
    {
        if (queItemList.Count > 0)
        {
            foreach (UI_BuildQueItem _queItem in queItemList)
            {
                Destroy(_queItem.gameObject);
            }
            queItemList.Clear();
        }
    }
}
// Writen by Lukasz Dziedziczak