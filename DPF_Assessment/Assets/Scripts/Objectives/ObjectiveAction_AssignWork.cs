using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAction_AssignWork : ObjectiveAction
{
    [SerializeField] List<WorkAssignment> workAssignments = new List<WorkAssignment>();

    [System.Serializable]
    public class WorkAssignment
    {
        public Unit unit;
        public CollectableResource resource;
    }

    public override void DoAction()
    {
        base.DoAction();
        AssignWork();
    }

    private void AssignWork()
    {
        if (workAssignments.Count > 0)
        {
            foreach (WorkAssignment workAssignment in workAssignments)
            {
                workAssignment.unit.SetTarget(workAssignment.resource);
            }    
        }
    }
}
