using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Objective : MonoBehaviour
{
    [SerializeField] protected bool isActivated = false;
    [SerializeField] protected bool isVisible = true;
    [SerializeField] protected string objectiveTitle;
    [SerializeField] protected string objectiveDescription;
    [SerializeField] protected float messageTimeDelay = 0;

    [Header("On Completion")]
    [SerializeField] protected string completionMessage = "";
    [SerializeField] protected float completionMessageTimeDelay = 0;
    [SerializeField] UnityEvent OnObjectiveComplete;
    [SerializeField] List<Objective> nextObjective = new List<Objective>();

    protected GameController gameController;
    protected ObjectiveManager objectiveManager;
    public List<ObjectiveAction> actionList = new List<ObjectiveAction>();

    protected void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        objectiveManager = GetComponentInParent<ObjectiveManager>();
        CreateActionList();
    }

    private void CreateActionList()
    {
        ObjectiveAction[] actions = GetComponents<ObjectiveAction>();

        if (actions.Length > 0)
        {
            foreach (ObjectiveAction action in actions)
            {
                actionList.Add(action);
            }
        }
        
    }


    // Start is called before the first frame update
    protected void Start()
    {
        //print(name + " is in scene");
        if (objectiveManager == null) Debug.LogError(name + " cannot find ObjectiveManager");
        if (isActivated) ShowObjectiveNotification();
    }

    // Update is called once per frame
    protected void Update()
    {
        
    }

    public void ObjectiveComplete()
    {
        //Debug.Log(name + " is complete");
        isActivated = false;
        OnObjectiveComplete.Invoke();
        RunObjectiveCompleteActions();
        ShowObjectiveCompleteNotification();
        ActivateNextObjectives();
    }

    private void ActivateNextObjectives()
    {
        if (nextObjective.Count > 0)
        {
            foreach (Objective objective in nextObjective)
            {
                objective.ActivateObjective();
            }
        }
    }

    private void RunObjectiveCompleteActions()
    {
        foreach(ObjectiveAction action in actionList)
        {
            if (action.onObjectiveComplete) action.DoAction();
        }
    }

    public void ActivateObjective()
    {
        isActivated = true;
        ShowObjectiveNotification();
        RunObjectiveActivateActions();
    }

    private void RunObjectiveActivateActions()
    {
        foreach (ObjectiveAction action in actionList)
        {
            if (action.onObjectiveActivate) action.DoAction();
        }
    }

    public void ShowObjectiveNotification()
    {
        if (isVisible)
        {
            objectiveManager.MessageSystem().ShowMessage("New Objective: " + objectiveTitle, messageTimeDelay);
        }
    }

    public void ShowObjectiveCompleteNotification()
    {
        if (isVisible)
        {
            objectiveManager.MessageSystem().ShowMessage("Objective Complete: " + objectiveTitle, messageTimeDelay);
        }

        if (completionMessage != "")
        {
            objectiveManager.MessageSystem().ShowMessage(completionMessage, completionMessageTimeDelay);
        }
    }

    public bool IsActivated { get { return isActivated; } }

    public bool IsVisible { get { return isVisible; } }

    public string Title { get { return objectiveTitle; } }

    public string Description {  get { return objectiveDescription; } }

    public void SetTitle(string newTitle)
    {
        objectiveTitle = newTitle;
    }
}
