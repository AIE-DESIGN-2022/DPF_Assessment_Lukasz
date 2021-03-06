// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private int playerNumber = 1;
    private List<Faction> factions = new List<Faction>();
    private HUD_Manager hud_Manager;
    private PlayerController playerController;
    private GameCameraController cameraController;
    private FogOfWarController fogOfWarController;
    private ObjectiveManager objectiveManager;
    private UI_Menu pauseMenu;
    private Transform mapTransform;
    private List<Selectable> nonPlayersSelectables = new List<Selectable>();
    private List<CollectableResource> collectableResources = new List<CollectableResource>();
    private StatSystem statSystem;
    private UI_MessageSystem messageSystem;
    private UI_Settings settingsUI;
    private UI_Objectives objectivesUI;

    [Header("New Game")]
    [SerializeField] private bool isNewGame = false;
    [SerializeField] List<Faction.EFaction> listOfPlayers;
    [SerializeField] int startingFood = 0;
    [SerializeField] int startingWood = 0;
    [SerializeField] int startingGold = 0;

    private void Awake()
    {
        hud_Manager = GetComponentInChildren<HUD_Manager>();
        playerController = GetComponentInChildren<PlayerController>();
        cameraController = GetComponentInChildren<GameCameraController>();
        fogOfWarController = GetComponentInChildren<FogOfWarController>();
        objectiveManager = GetComponentInChildren<ObjectiveManager>();
        statSystem = GetComponent<StatSystem>();
        messageSystem = GetComponentInChildren<UI_MessageSystem>();
        FindMenus();
    }

    private void FindMenus()
    {
        UI_Menu[] menus = FindObjectsOfType<UI_Menu>();
        foreach (UI_Menu menu in menus)
        {
            if (menu.name == "PauseMenu") pauseMenu = menu;
        }

        settingsUI = GetComponentInChildren<UI_Settings>();
        objectivesUI = GetComponentInChildren<UI_Objectives>();

    }

/*    public UI_Menu PauseMenu()
    {
        if (pauseMenu != null) return pauseMenu;
        else
        {
            Debug.LogError(name + " cannot find pause menu.");
            return null;
        }
    }*/

    public UI_Menu PauseMenu {  get {  return pauseMenu; } }

    public UI_Settings SettingsMenu { get { return settingsUI; } }

    public UI_Objectives ObjectivesMenu { get { return objectivesUI; } }


    // Start is called before the first frame update
    void Start()
    {
        if (isNewGame) SetupNewGame();
        else SetupGame();
        BuildListOfNonPlayerSelectable();
        BuildListOfCollectableResources();
        CollectableResource.onResourceDepleted += ResourceConsumed;
        if (pauseMenu != null) pauseMenu.Show(false);
    }

    private void BuildListOfNonPlayerSelectable()
    {
        Selectable[] selectables = FindObjectsOfType<Selectable>();
        foreach (Selectable selectable in selectables)
        {
            if (selectable.PlayerNumber() == 0) nonPlayersSelectables.Add(selectable);
        }
    }

    private void BuildListOfCollectableResources()
    {
        collectableResources.Clear();
        CollectableResource[] resources = FindObjectsOfType<CollectableResource>();
        foreach(CollectableResource resource in resources)
        {
            collectableResources.Add(resource);
        }
    }

    private List<CollectableResource> ListOfCollectableResources()
    {
        if (collectableResources.Count == 0) BuildListOfCollectableResources();

        List<CollectableResource> list = new List<CollectableResource>();

        foreach (CollectableResource collectable in collectableResources.ToArray())
        {
            Building farm = collectable.GetComponent<Building>();
            if (collectable != null && collectable.HasResource() && farm == null) list.Add(collectable);
        }

        return list;
    }

    public List<Selectable> AllSelectablesButPlayers()
    {
        List<Selectable> list = new List<Selectable>();

        foreach (Faction faction in factions)
        {
            if (faction.PlayerNumber() != playerNumber)
            {
                foreach (Selectable selectable in faction.Selectables())
                {
                    list.Add(selectable);
                }

            }
        }

        foreach (Selectable selectable in nonPlayersSelectables)
        {
            list.Add(selectable);
        }

        return list;
    }

    private void SetupNewGame()
    {
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
        if (spawnPoints.Length != listOfPlayers.Count) Debug.LogError(name + " missmatch in number of players and number of spawn points");

        for (int i = 0; i < listOfPlayers.Count; i++)
        {
            //GameObject newObject = Instantiate(new GameObject(), spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
            GameObject newObject = new GameObject();
            newObject.AddComponent<Faction>();
            Faction newFaction = newObject.GetComponent<Faction>();
            newFaction.SetupFaction(listOfPlayers[i], i + 1);
            newFaction.transform.position = spawnPoints[i].transform.position;
            newFaction.SpawnFirstBuilding(spawnPoints[i].GroundCoordinates());
            factions.Add(newFaction);
            newFaction.SetGameController(this);
            GiveStartingResources(newFaction);

            if (i == 0)
            {
                hud_Manager.SetPlayerFaction(newFaction);
                cameraController.SetPlayerFaction(newFaction);
                cameraController.MoveTo(spawnPoints[i].GroundCoordinates());
                fogOfWarController.SetPlayerFaction(newFaction);
            }

            if (i != 0)
            {
                newObject.AddComponent<EnemyAIController>();
            }
        }
        playerController.SetHUD_Manager(hud_Manager);

        objectiveManager.MakeObjectives(GetNonPlayerFactions());
    }

    public void FactionDefated(Faction faction)
    {
        if (faction.PlayerNumber() == playerNumber)
        {
            EndGame(false);
        }
        else if (faction.PlayerNumber() != playerNumber)
        {
            factions.Remove(faction);
            Destroy(faction.gameObject);

            /*if (factions.Count == 1)
            {
                EndGame(true);
            }*/
        }
    }

    public void EndGame(bool playerWon)
    {
        statSystem.EndGame(playerWon);
        SceneManager.LoadScene("EndScreen");
    }


    private void SetupGame()
    {
        BuildListOfFactions();
        hud_Manager.SetPlayerFaction(GetPlayerFaction());
        cameraController.SetPlayerFaction(GetPlayerFaction());
        playerController.SetHUD_Manager(hud_Manager);
        fogOfWarController.SetPlayerFaction(GetPlayerFaction());
    }

    private void BuildListOfFactions()
    {
        Faction[] newFactions = FindObjectsOfType<Faction>();
        foreach (Faction faction in newFactions)
        {
            //Debug.Log(name + " found " + faction.ToString() + " with FactionType " + faction.FactionType().ToString());
            factions.Add(faction);
            faction.SetGameController(this);

        }

        if (factions.Count == 0 && !isNewGame) Debug.LogError(name + " found no factions at game start.");

        objectiveManager.MakeObjectives(GetNonPlayerFactions());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void AddFaction()
    {
        GameObject newFaction = Instantiate(new GameObject(), transform);
        newFaction.AddComponent<Faction>();
        Faction faction = newFaction.GetComponent<Faction>();
        //faction.SetFaction();
    }

    public Faction GetFaction(int playerNumber)
    {
        if (factions.Count > 0)
        {
            foreach (Faction faction in factions)
            {
                if (faction.PlayerNumber() == playerNumber)
                {
                    return faction;
                }
            }

            Debug.LogError("GameController canot not find faction with PlayerNumber = " + playerNumber);
            return null;
        }
        else
        {
            Debug.LogError("GameController has no list of factions.");
            return null;
        }

        
    }

    public bool IsPlayerNumber(int aPlayerNumber)
    {
        return aPlayerNumber == playerNumber;
    }

    public Faction GetPlayerFaction()
    {
        return GetFaction(playerNumber);
    }

    public HUD_Manager HUD_Manager()
    {
        if (hud_Manager != null) return hud_Manager;
        else return null;
    }

    public bool IsPlayerFaction(int factionNumber)
    {
        return playerNumber == factionNumber;
    }

    public PlayerController PlayerController() { return playerController; }

    public GameCameraController CameraController() { return cameraController; }

    private void GiveStartingResources(Faction faction)
    {
        faction.AddToStockpile(CollectableResource.EResourceType.Wood, startingWood);
        faction.AddToStockpile(CollectableResource.EResourceType.Food, startingFood);
        faction.AddToStockpile(CollectableResource.EResourceType.Gold, startingGold);
    }

    public Transform GetMapTransform()
    {
        if (mapTransform == null)
        {
            Transform[] transforms = FindObjectsOfType<Transform>();
            foreach (Transform t in transforms)
            {
                if (t.gameObject.name == "Map") mapTransform = t;
            }
        }

        return mapTransform;
    }

    public List<Faction> GetNonPlayerFactions()
    {
        List<Faction> list = new List<Faction>();

        foreach (Faction faction in factions)
        {
            if (faction.PlayerNumber() != playerNumber) list.Add(faction);
        }

        return list;
    }

    public CollectableResource NearbyResource(Vector3 position, float sightRadious, CollectableResource.EResourceType type)
    {
        CollectableResource nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (CollectableResource resource in ListOfCollectableResources())
        {
            float distance = Vector3.Distance(position, resource.transform.position);
            
            if (distance < sightRadious && resource.ResourceType() == type && !resource.HasCollector)
            {
                if (distance < nearestDistance)
                {
                    nearest = resource;
                    nearestDistance = distance;
                }
            }
        }

        return nearest;
    }

    public UI_MessageSystem MessageSystem { get { return messageSystem; } }

    public void ResourceConsumed(CollectableResource collectableResource)
    {
        if (collectableResources.Contains(collectableResource))
        {
            collectableResources.Remove(collectableResource);
        }
    }
}
// Writen by Lukasz Dziedziczak