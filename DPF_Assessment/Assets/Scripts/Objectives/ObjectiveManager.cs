using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private List<Objective> objectives = new List<Objective>();
    [SerializeField] private ObjectiveKillFaction killFactionObjectivePrefab;

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
        CheckIfObjectivesComplete();
    }

    private void CheckIfObjectivesComplete()
    {
        if (objectives.Count > 0)
        {
            bool allComplete = true;

            foreach(Objective obj in objectives)
            {
                if (obj.IsActivated) allComplete = false;
            }

            if (allComplete)
            {
                FindObjectOfType<GameController>().EndGame(true);
            }
        }
    }

    public UI_MessageSystem MessageSystem() { return messageSystem; }

    public List<Objective> Objectives { get { return objectives; } }

    public void MakeObjectives(List<Faction> factions)
    {
        if (objectives.Count > 0) return;

        if (factions.Count == 0) Debug.LogError(name + " no factions to make objectives.");

        foreach (Faction faction in factions)
        {
            ObjectiveKillFaction objectiveKillFaction = Instantiate(killFactionObjectivePrefab, transform);
            objectives.Add(objectiveKillFaction);
            objectiveKillFaction.SetFaction(faction);
            objectiveKillFaction.ActivateObjective();
        }

        if (objectives.Count == 0) Debug.LogError(name + " has no objectives");
    }
}
