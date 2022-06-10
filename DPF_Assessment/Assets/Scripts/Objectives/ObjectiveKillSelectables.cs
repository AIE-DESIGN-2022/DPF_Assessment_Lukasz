using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveKillSelectables : Objective
{
    [SerializeField] private List<Selectable> selectablesToKill;


    // Start is called before the first frame update
    private new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    private new void Update()
    {
        CheckIfSelectableAlive();
        CheckIfAnySelectableLeft();
    }

    private void CheckIfAnySelectableLeft()
    {
        if (!isActivated) return;

        if (selectablesToKill.Count == 0) ObjectiveComplete();  
    }

    private void CheckIfSelectableAlive()
    {
        if (!isActivated) return;

        Selectable[] list = selectablesToKill.ToArray();

        foreach (Selectable selectable in list)
        {
            if (!selectable.IsAlive()) selectablesToKill.Remove(selectable);
        }
    }
}
