using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveCreateUnit : Objective
{
    [Header("Create Unit")]
    [SerializeField] private Unit.EUnitType unitType;
    [SerializeField] private int amount = 1;

    private int unitsCreated = 0;

    private new void Start()
    {
        base.Start();
        Faction.newUnitCreated += NewUnitCreated;
    }


    private void NewUnitCreated(Unit newUnit)
    {
        if (!isActivated) return;

        if (newUnit.UnitType() == unitType)
        {
            unitsCreated++;

            if (unitsCreated == amount)
            {
                ObjectiveComplete();
            }
        }
    }
}
