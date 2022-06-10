using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private List<Objective> objectives = new List<Objective>();

    private UI_MessageSystem messageSystem;

    private void Awake()
    {
        messageSystem = FindObjectOfType<UI_MessageSystem>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (messageSystem == null) Debug.LogError(name + " cannot find UI_MessageSystem");
        FindObjectives();
    }

    private void FindObjectives()
    {
        Objective[] objectiveChildren = GetComponentsInChildren<Objective>();
        foreach (Objective obj in objectiveChildren)
        {
            objectives.Add(obj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public UI_MessageSystem MessageSystem() { return messageSystem; }

    public List<Objective> Objectives { get { return objectives; } }
}
