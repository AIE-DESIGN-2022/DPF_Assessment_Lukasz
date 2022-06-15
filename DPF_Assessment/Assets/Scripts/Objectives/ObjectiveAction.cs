using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAction : MonoBehaviour
{
    public bool onObjectiveComplete = false;
    public bool onObjectiveActivate = false;

    protected GameController gameController;

    private void Awake()
    {
        gameController = FindObjectOfType<GameController>();
    }

    public virtual void DoAction()
    {
        // any universal actions to all types of Objective Actions go here.
    }
}
