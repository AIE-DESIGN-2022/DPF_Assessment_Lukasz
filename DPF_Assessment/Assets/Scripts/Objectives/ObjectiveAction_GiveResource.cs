using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAction_GiveResource : ObjectiveAction
{
    [SerializeField] private int food;
    [SerializeField] private int wood;
    [SerializeField] private int gold;

    public override void DoAction()
    {
        base.DoAction();

        GiveResource();
    }

    private void GiveResource()
    {
        Faction playerFaction = gameController.GetPlayerFaction();

        playerFaction.AddToStockpile(CollectableResource.EResourceType.Food, food);
        playerFaction.AddToStockpile(CollectableResource.EResourceType.Wood, wood);
        playerFaction.AddToStockpile(CollectableResource.EResourceType.Gold, gold);
    }
}
