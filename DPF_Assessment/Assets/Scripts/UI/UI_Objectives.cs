using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Objectives : MonoBehaviour
{
    private ObjectiveManager objectiveManager;
    private UI_Objective objectivePrefab;
    private List<UI_Objective> listOfObjectives = new List<UI_Objective> ();
    [SerializeField] private GameObject ui_list;

    private void Awake()
    {
        objectiveManager = FindObjectOfType<ObjectiveManager>();
        objectivePrefab = (UI_Objective)Resources.Load<UI_Objective>("HUD_Prefabs/UI_Objective");
    }

    // Start is called before the first frame update
    void Start()
    {
        Show(false);
        if (objectiveManager == null) Debug.LogError(name + " cannot find ObjectiveManager");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleShowing()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            listOfObjectives.Clear();
        }
        else
        {
            gameObject.SetActive(true);
            UpdateObjectiveList();
        }
    }

    public void Show(bool isShowing = true)
    {
        if (isShowing && !gameObject.activeSelf)
        {
            ToggleShowing();
            
        }
        else if (!isShowing && gameObject.activeSelf)
        {
            ToggleShowing();
            
        }
    }

    public void UpdateObjectiveList()
    {
        foreach (Objective objective in objectiveManager.Objectives)
        {
            if (objective.IsActivated && objective.IsVisible)
            {
                UI_Objective ui_Objective = Instantiate(objectivePrefab, ui_list.transform);
                ui_Objective.Set(objective.Title, objective.Description);
                listOfObjectives.Add(ui_Objective);
            }
        }

    }
  
}
