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

    [SerializeField] UnityEvent OnObjectiveComplete;

    protected GameController gameController;
    protected ObjectiveManager objectiveManager;

    protected void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        objectiveManager = GetComponentInParent<ObjectiveManager>();
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
        Debug.Log(name + " is complete");
        isActivated = false;
        OnObjectiveComplete.Invoke();
    }

    public void ActivateObjective()
    {
        isActivated = true;
        ShowObjectiveNotification();
    }

    public void ShowObjectiveNotification()
    {
        //print(name + " showing notification");
        objectiveManager.MessageSystem().ShowMessage("New Objective: " + objectiveTitle, messageTimeDelay);
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
