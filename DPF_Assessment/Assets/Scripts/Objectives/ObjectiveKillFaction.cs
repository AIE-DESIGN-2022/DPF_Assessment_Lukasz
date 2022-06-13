using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveKillFaction : Objective
{
    [SerializeField] private Faction faction;

    // Start is called before the first frame update
    private new void Start()
    {
        
    }

    // Update is called once per frame
    private new void Update()
    {
        base.Update();
        CheckIfFactionIsAlive();
    }

    private void CheckIfFactionIsAlive()
    {
        if (!isActivated) return;

        if (faction == null) ObjectiveComplete();
    }

    public void SetFaction(Faction newFaction)
    {
        faction = newFaction;
        SetTitle("Destroy the " + faction.FactionType().ToString() + "s");
    }
}
