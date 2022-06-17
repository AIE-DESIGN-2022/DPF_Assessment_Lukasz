using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAction_RemoveObject : ObjectiveAction
{
    [SerializeField] private List<GameObject> objects = new List<GameObject>();

    public override void DoAction()
    {
        base.DoAction();
        RemoveObjects();
    }

    private void RemoveObjects()
    {
        foreach (GameObject obj in objects.ToArray())
        {
            Destroy(obj);
        }
    }
}
